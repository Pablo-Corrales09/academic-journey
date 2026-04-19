using System;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.DbModel;
using HotelCarga.ApiModel.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelCarga.ApiModel.Controllers;

/// <summary>
/// Orchestrates booking creation with availability checking and waiting queue fallback.
/// Implements the business logic:
/// 1. If selected room is AVAILABLE: create booking immediately
/// 2. If selected room is OCCUPIED: suggest first AVAILABLE room in same category
/// 3. If all rooms occupied: create waiting queue entry and PENDING booking
/// </summary>
[Route("[controller]")]
public class BookingAvailabilityController : BaseApiController
{
    private readonly ILogger<BookingAvailabilityController>? _logger;

    public BookingAvailabilityController(HotelCargaContext? dbContext = null, ILogger<BookingAvailabilityController>? logger = null)
        : base(dbContext, null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a booking with availability-aware fallback logic.
    /// Returns structured response indicating booking creation, alternative room suggestion, or queue entry.
    /// </summary>
    [HttpPost("CreateWithAvailabilityFlow")]
    public async Task<IActionResult> CreateWithAvailabilityFlow([FromBody] BookingAvailabilityRequest request)
    {
        if (DbContext is null)
            return DbBackendMissing();

        // Validate request
        var (valid, error) = request.Validate();
        if (!valid)
            return BadRequest(new BookingAvailabilityResponse
            {
                result_type = "VALIDATION_ERROR",
                success = false,
                message = error ?? "Invalid request.",
                validation_error = new ValidationErrorPayload
                {
                    error_code = "INVALID_REQUEST",
                    error_message = error ?? "Invalid request parameters."
                }
            });

        try
        {
            // Verify customer exists
            var customer = await DbContext.Set<customer>().FindAsync(request.customer_id);
            if (customer == null)
            {
                return NotFound(new BookingAvailabilityResponse
                {
                    result_type = "VALIDATION_ERROR",
                    success = false,
                    message = "Customer not found.",
                    validation_error = new ValidationErrorPayload
                    {
                        error_code = "CUSTOMER_NOT_FOUND",
                        error_message = "El cliente seleccionado no existe."
                    }
                });
            }

            // Verify selected room exists
            var selectedRoom = await DbContext.Set<room>()
                .Include(r => r.status)
                .Include(r => r.category)
                .FirstOrDefaultAsync(r => r.id == request.selected_room_id);

            if (selectedRoom == null)
            {
                return NotFound(new BookingAvailabilityResponse
                {
                    result_type = "VALIDATION_ERROR",
                    success = false,
                    message = "Selected room not found.",
                    validation_error = new ValidationErrorPayload
                    {
                        error_code = "ROOM_NOT_FOUND",
                        error_message = "La habitación seleccionada no existe."
                    }
                });
            }

            // Check if selected room is bookable
            bool selectedRoomIsAvailable = await IsRoomAvailableForDateRangeAsync(
                request.selected_room_id,
                request.check_in,
                request.check_out);

            // If selected room is available and not out of order: create booking
            if (selectedRoomIsAvailable && selectedRoom.status_id != 3)
            {
                return await CreateBookingAsync(request, customer, selectedRoom);
            }

            // Selected room is not available or out of order
            // Rule 1: Search for alternative AVAILABLE room in same category
            byte categoryId = selectedRoom.category_id;
            var alternativeRoom = await FindFirstAvailableRoomInCategoryAsync(
                categoryId,
                request.check_in,
                request.check_out,
                request.selected_room_id); // Exclude the originally selected room

            if (alternativeRoom != null)
            {
                // Return alternative room suggestion
                return Ok(new BookingAvailabilityResponse
                {
                    result_type = "ALTERNATIVE_ROOM_SUGGESTED",
                    success = false,
                    message = "La habitación seleccionada no está disponible, pero encontramos una alternativa en la misma categoría.",
                    alternative_room_suggested = new AlternativeRoomPayload
                    {
                        original_requested_room_id = request.selected_room_id,
                        original_requested_room_number = selectedRoom.room_number,
                        room_category_name = selectedRoom.category?.category_name ?? "Unknown",
                        alternative_room_id = alternativeRoom.id,
                        alternative_room_number = alternativeRoom.room_number,
                        alternative_room_status_id = alternativeRoom.status_id,
                        alternative_room_status_name = alternativeRoom.status?.status_name ?? "UNKNOWN",
                        alternative_nightly_rate = alternativeRoom.nightly_rate,
                        floor_number = alternativeRoom.floor_number ?? 0
                    }
                });
            }

            // Rule 2: No alternative rooms available - add to waiting queue
            // Create both waiting_queue and pending booking in single transaction
            return await CreateWaitingQueueAndPendingBookingAsync(request, customer, selectedRoom);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unhandled error in CreateWithAvailabilityFlow for customer {customerId}", request.customer_id);
            return StatusCode(500, new BookingAvailabilityResponse
            {
                result_type = "VALIDATION_ERROR",
                success = false,
                message = "No fue posible procesar su solicitud en este momento. Por favor, inténtelo de nuevo.",
                validation_error = new ValidationErrorPayload
                {
                    error_code = "PROCESSING_ERROR",
                    error_message = "An unexpected error occurred while processing the booking request."
                }
            });
        }
    }

    /// <summary>
    /// Confirms an alternative room suggestion by creating a booking with the suggested room.
    /// </summary>
    [HttpPost("ConfirmAlternativeRoom")]
    public async Task<IActionResult> ConfirmAlternativeRoom([FromBody] ConfirmAlternativeRoomRequest request)
    {
        if (DbContext is null)
            return DbBackendMissing();

        try
        {
            var customer = await DbContext.Set<customer>().FindAsync(request.customer_id);
            if (customer == null)
                return NotFound(new { error = true, message = "Customer not found." });

            var room = await DbContext.Set<room>()
                .Include(r => r.status)
                .Include(r => r.category)
                .FirstOrDefaultAsync(r => r.id == request.alternative_room_id);

            if (room == null)
                return NotFound(new { error = true, message = "Alternative room not found." });

            // Verify room is still available
            bool isAvailable = await IsRoomAvailableForDateRangeAsync(
                request.alternative_room_id,
                request.check_in,
                request.check_out);

            if (!isAvailable)
            {
                return Conflict(new BookingAvailabilityResponse
                {
                    result_type = "VALIDATION_ERROR",
                    success = false,
                    message = "La habitación alternativa ya no está disponible. Por favor, intenta de nuevo.",
                    validation_error = new ValidationErrorPayload
                    {
                        error_code = "ROOM_NO_LONGER_AVAILABLE",
                        error_message = "The alternative room is no longer available."
                    }
                });
            }

            return await CreateBookingAsync(
                new BookingAvailabilityRequest(request.customer_id, request.alternative_room_id, request.check_in, request.check_out),
                customer,
                room);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error confirming alternative room for customer {customerId}", request.customer_id);
            return StatusCode(500, new BookingAvailabilityResponse
            {
                result_type = "VALIDATION_ERROR",
                success = false,
                message = "No fue posible confirmar la habitación alternativa. Por favor, inténtelo de nuevo.",
                validation_error = new ValidationErrorPayload
                {
                    error_code = "CONFIRM_ALTERNATIVE_FAILED",
                    error_message = "Alternative room confirmation failed due to a server error."
                }
            });
        }
    }

    private async Task<bool> IsRoomAvailableForDateRangeAsync(uint roomId, DateTime checkIn, DateTime checkOut)
    {
        // Check if room is out of service
        var room = await DbContext!.Set<room>().FindAsync(roomId);
        if (room?.status_id == 3) // OUT OF ORDER
            return false;

        // Check for booking conflicts (overlapping PENDING or CONFIRMED bookings)
        bool hasConflict = await DbContext!.Set<booking>()
            .AnyAsync(b =>
                b.room_id == roomId
                && b.status_id != 3 // Exclude CANCELED bookings
                && b.check_in < checkOut
                && b.check_out > checkIn);

        return !hasConflict;
    }

    private async Task<room?> FindFirstAvailableRoomInCategoryAsync(
        byte categoryId,
        DateTime checkIn,
        DateTime checkOut,
        uint excludeRoomId = 0)
    {
        // Get all rooms in category that are not out of order
        var candidateRooms = await DbContext!.Set<room>()
            .Where(r => r.category_id == categoryId && r.status_id != 3 && r.id != excludeRoomId)
            .Include(r => r.status)
            .Include(r => r.category)
            .OrderBy(r => r.room_number)
            .ToListAsync();

        // Find first room with no booking conflicts
        foreach (var room in candidateRooms)
        {
            bool hasConflict = await DbContext!.Set<booking>()
                .AnyAsync(b =>
                    b.room_id == room.id
                    && b.status_id != 3 // Exclude CANCELED
                    && b.check_in < checkOut
                    && b.check_out > checkIn);

            if (!hasConflict)
                return room;
        }

        return null;
    }

    private async Task<IActionResult> CreateBookingAsync(
        BookingAvailabilityRequest request,
        customer customer,
        room room)
    {
        using var transaction = await DbContext!.Database.BeginTransactionAsync();

        try
        {
            // Create booking with CONFIRMED status (status_id = 2)
            var booking = new booking
            {
                customer_id = request.customer_id,
                room_id = request.selected_room_id,
                check_in = request.check_in,
                check_out = request.check_out,
                status_id = 2, // CONFIRMED
                nightly_rate = room.nightly_rate,
                total_price = CalculateTotalPrice(room.nightly_rate, request.check_in, request.check_out),
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            booking.reserve_number = await AssignReserveNumberAsync();

            await DbContext!.Set<booking>().AddAsync(booking);
            await DbContext!.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger?.LogInformation(
                "Booking {bookingId} created for customer {customerId} room {roomId}",
                booking.id, request.customer_id, request.selected_room_id);

            return Ok(new BookingAvailabilityResponse
            {
                result_type = "BOOKING_CREATED",
                success = true,
                message = "Reserva creada exitosamente. Tu número de reserva es: " + booking.reserve_number,
                booking_created = new BookingDataPayload
                {
                    id = booking.id,
                    reserve_number = booking.reserve_number,
                    status_id = booking.status_id,
                    status_name = "CONFIRMED",
                    customer_id = booking.customer_id,
                    room_id = booking.room_id ?? 0,
                    room_number = room.room_number,
                    check_in = booking.check_in,
                    check_out = booking.check_out,
                    nightly_rate = booking.nightly_rate,
                    total_price = booking.total_price,
                    created_at = booking.created_at
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger?.LogError(ex, "Error creating booking for customer {customerId}", request.customer_id);
            return StatusCode(500, new BookingAvailabilityResponse
            {
                result_type = "VALIDATION_ERROR",
                success = false,
                message = "No fue posible crear la reserva. Por favor, verifique las fechas seleccionadas e inténtelo de nuevo.",
                validation_error = new ValidationErrorPayload
                {
                    error_code = "BOOKING_CREATION_FAILED",
                    error_message = "Booking creation failed due to a server error."
                }
            });
        }
    }

    private async Task<IActionResult> CreateWaitingQueueAndPendingBookingAsync(
        BookingAvailabilityRequest request,
        customer customer,
        room room)
    {
        using var transaction = await DbContext!.Database.BeginTransactionAsync();

        try
        {
            // Get queue status ID for PENDING
            var pendingQueueStatus = await DbContext!.Set<queue_status>()
                .FirstOrDefaultAsync(qs => qs.status_name == "PENDING");
            if (pendingQueueStatus == null)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new BookingAvailabilityResponse
                {
                    result_type = "VALIDATION_ERROR",
                    success = false,
                    message = "Queue status configuration error.",
                    validation_error = new ValidationErrorPayload
                    {
                        error_code = "QUEUE_STATUS_NOT_FOUND",
                        error_message = "PENDING queue status not found in database."
                    }
                });
            }

            // Create waiting queue entry
            var queueEntry = new waiting_queue
            {
                customer_id = request.customer_id,
                room_category_id = room.category_id,
                status_id = pendingQueueStatus.id,
                requested_check_in = request.check_in,
                check_out = request.check_out,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await DbContext!.Set<waiting_queue>().AddAsync(queueEntry);
            await DbContext!.SaveChangesAsync();

            booking? pendingBooking = null;
            try
            {
                // Best-effort pending booking with NULL room_id for deferred assignment.
                // Some deployed databases may still enforce room_id NOT NULL; in that case
                // we keep the queue registration successful and continue.
                pendingBooking = new booking
                {
                    customer_id = request.customer_id,
                    room_id = null,
                    check_in = request.check_in,
                    check_out = request.check_out,
                    status_id = 1,
                    nightly_rate = room.nightly_rate,
                    total_price = CalculateTotalPrice(room.nightly_rate, request.check_in, request.check_out),
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };

                pendingBooking.reserve_number = await AssignReserveNumberAsync();
                await DbContext!.Set<booking>().AddAsync(pendingBooking);
                await DbContext!.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (pendingBooking is not null)
                {
                    DbContext.Entry(pendingBooking).State = EntityState.Detached;
                }

                _logger?.LogWarning(ex,
                    "Pending booking could not be created for customer {customerId}; waiting queue entry {queueId} was still created.",
                    request.customer_id,
                    queueEntry.id);
            }

            await transaction.CommitAsync();

            if (pendingBooking is not null)
            {
                _logger?.LogInformation(
                    "Waiting queue {queueId} and pending booking {bookingId} created for customer {customerId} category {categoryId}",
                    queueEntry.id, pendingBooking.id, request.customer_id, room.category_id);
            }
            else
            {
                _logger?.LogInformation(
                    "Waiting queue {queueId} created for customer {customerId} category {categoryId} without pending booking.",
                    queueEntry.id, request.customer_id, room.category_id);
            }

            return Conflict(new BookingAvailabilityResponse
            {
                result_type = "QUEUED_AND_PENDING_BOOKING_CREATED",
                success = true,
                message = pendingBooking is null
                    ? "Todas las habitaciones en esta categoría están ocupadas. Se ha registrado su solicitud en la lista de espera."
                    : "Todas las habitaciones en esta categoría están ocupadas. Se ha registrado su solicitud en la lista de espera.",
                queued_and_pending = new QueuedAndPendingPayload
                {
                    waiting_queue_id = queueEntry.id,
                    request_number = FormatRequestNumber(queueEntry.id),
                    pending_booking_id = pendingBooking?.id ?? 0,
                    pending_reserve_number = pendingBooking?.reserve_number,
                    room_category_name = room.category?.category_name ?? "Unknown",
                    requested_check_in = queueEntry.requested_check_in,
                    requested_check_out = queueEntry.check_out ?? DateTime.UtcNow,
                    queue_status_name = "PENDING",
                    created_at = queueEntry.created_at
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger?.LogError(ex, "Error creating waiting queue for customer {customerId}", request.customer_id);
            return StatusCode(500, new BookingAvailabilityResponse
            {
                result_type = "VALIDATION_ERROR",
                success = false,
                message = "No fue posible registrar la solicitud en la lista de espera. Por favor, inténtelo de nuevo.",
                validation_error = new ValidationErrorPayload
                {
                    error_code = "QUEUE_CREATION_FAILED",
                    error_message = "Waiting queue creation failed due to a server error."
                }
            });
        }
    }

    private decimal CalculateTotalPrice(decimal nightlyRate, DateTime checkIn, DateTime checkOut)
    {
        if (nightlyRate == 0) return 0;
        int days = (checkOut - checkIn).Days;
        if (days <= 0) days = 1;
        return days * nightlyRate;
    }

    private async Task<string> AssignReserveNumberAsync()
    {
        var lastReservation = await DbContext!.Set<booking>()
            .OrderByDescending(b => b.reserve_number)
            .Select(b => b.reserve_number)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastReservation))
            return "RES0001";

        if (!lastReservation.StartsWith("RES"))
            throw new FormatException($"Invalid reserve number format: {lastReservation}");

        if (int.TryParse(lastReservation.Substring(3), out int number))
        {
            number++;
            return $"RES{number:D4}";
        }

        throw new FormatException($"Invalid numeric part in reserve number: {lastReservation}");
    }

    private static string FormatRequestNumber(uint id)
    {
        var token = unchecked(id * 2654435761u);
        return $"RQ-{token:X8}";
    }
}

/// <summary>
/// Request to confirm an alternative room suggestion.
/// </summary>
public record ConfirmAlternativeRoomRequest(
    uint customer_id,
    uint alternative_room_id,
    DateTime check_in,
    DateTime check_out);
