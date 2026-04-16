using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelCargaContext = HotelCarga.DbModel.HotelCargaContext;
using HotelCarga.HotelCarga.DbModel.Entities;
using HotelCarga.DbModel;
using HotelCargaJsonRepositoryModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelCarga.ApiModel.Services;
using Microsoft.Extensions.Logging;

namespace HotelCarga.ApiModel.Controllers;

// DTO for creating bookings - matches client's SaveBookingDto
public record SaveBookingDto(uint customer_id, uint room_id, DateTime check_in, DateTime check_out, byte? status_id = null);

[Route("[controller]")]
public class BookingController : BaseApiController
{
    private readonly BookingAlertService? _alertService;
    private readonly ILogger<BookingController>? _logger;

    // Constructor with dependency injection for alert service and logger
    public BookingController(HotelCargaContext? dbContext = null, JsonDataContext? jsonContext = null, BookingAlertService? alertService = null, ILogger<BookingController>? logger = null)
        : base(dbContext, jsonContext)
    {
        _alertService = alertService;
        _logger = logger;
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetById(uint id, bool useJson = false)
    {
        booking? bookingItem = null;
        if (UseJsonBackend(useJson))
        {
            bookingItem = JsonContext?.bookings.FirstOrDefault(b => b.id == id);
            if (bookingItem != null)
            {
                var status = JsonContext?.booking_statuses.FirstOrDefault(s => s.id == bookingItem.status_id);
                if (status is not null) bookingItem.status = status;

                var customer = JsonContext?.customers.FirstOrDefault(c => c.id == bookingItem.customer_id);
                if (customer is not null) bookingItem.customer = customer;

                var room = JsonContext?.rooms.FirstOrDefault(r => r.id == bookingItem.room_id);
                if (room is not null) bookingItem.room = room;
            }
        }
        else
        {
           if(DbContext is null) return DbBackendMissing();
           bookingItem = await DbContext.Set<booking>()           
            .Include(b => b.status)
            .Include(b => b.customer)
            .Include(b => b.room)
            .FirstOrDefaultAsync(b => b.id == id);
        }
        if (bookingItem is null) return NotFound();

        return Ok(BuildBookingResponse(bookingItem));
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var bookings = JsonContext!.bookings;
            foreach (var bookingItem in bookings)
            {
                var status = JsonContext.booking_statuses.FirstOrDefault(s => s.id == bookingItem.status_id);
                var customer =  JsonContext.customers.FirstOrDefault(c => c.id == bookingItem.customer_id);
                var room = JsonContext.rooms.FirstOrDefault(r => r.id == bookingItem.room_id);
            }
            return Ok(bookings.Select(BuildBookingResponse).ToList());
            }
            if(DbContext is null) return DbBackendMissing();
            var entities = await DbContext.Set<booking>()
            .Include(c=> c.customer)
            .Include(c=> c.room)
            .Include(c=> c.status)
            .ToListAsync();
            return Ok(entities.Select(BuildBookingResponse).ToList());
    }

   [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] SaveBookingDto dto, bool useJson = false, bool addToQueueIfUnavailable = true)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();
        // Verify that the dates of stay are correct
        if(!AreDatesValid(dto.check_in, dto.check_out, out string errorMessage)) return Conflict(new { error = true, message = errorMessage });

        using var transaction = await DbContext.Database.BeginTransactionAsync();

        try
        {
            // Retrieve the room to check availability and price
            var room = await DbContext.Set<room>().FindAsync(dto.room_id);
            
            // Verify room exists
            if (room == null)
            {
                await transaction.RollbackAsync();
                return NotFound(new { error = true, message = "La habitación seleccionada no existe." });
            }

            // Check room availability status
            bool isAvailable = IsRoomAvailable(room, dto.room_id, out errorMessage);

            // If room is not available and addToQueueIfUnavailable is true, add to waiting queue instead
            if (!isAvailable && addToQueueIfUnavailable)
            {
                await transaction.RollbackAsync();
                return await AddToWaitingQueue(dto);
            }

            // If room is not available and not adding to queue, return error
            if (!isAvailable)
            {
                await transaction.RollbackAsync();
                return Conflict(new { error = true, message = errorMessage });
            }

            // Create the booking entity from DTO
            var item = new booking
            {
                customer_id = dto.customer_id,
                room_id = dto.room_id,
                check_in = dto.check_in,
                check_out = dto.check_out
            };

            // Prepare the reservation data
            item.reserve_number = await AssignReserveNumber();
            item.nightly_rate = room!.nightly_rate;

            // Calculate total price based on room's nightly rate.
            item.total_price = calculate_total_price(room.nightly_rate, item.check_in, item.check_out);

            //Booking status update
            item.status_id = 2; 
            await DbContext.Set<booking>().AddAsync(item);
            await DbContext.SaveChangesAsync();

            //Room status update  
            room.status_id = 2; // Occupied
            await DbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return CreatedAtAction(
            nameof(GetById), 
            new { id = item.id }, 
            new { 
            success = true,
            message = "Reserva creada exitosamente. Tu número de reserva es:  " + item.reserve_number,
            data = BuildBookingResponse(item) 
            });
        }
        catch (Exception ex)
        {
            string realError = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { error = true, message = "Processing error: " + realError });
        }
    }

    //Crea el número de reserva, siguiendo un consecutivo.
    private async Task<string> AssignReserveNumber()
    {
        try
        {
            var lastReservation = await DbContext!.bookings
                .OrderByDescending(b => b.reserve_number)
                .Select(b => b.reserve_number)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastReservation))
            {
                return "RES0001";
            }

            //Valida que el consetuvito comience con el prefijo RES
            if (!lastReservation.StartsWith("RES"))
            {
                throw new FormatException($"El código '{lastReservation}' no tiene el prefijo esperado 'RES'.");
            }

            string numericPart = lastReservation.Substring(3);

            if (int.TryParse(numericPart, out int number))
            {
                number++;
                return $"RES{number:D4}";
            }
            else
            {
                throw new FormatException($"La parte numérica de '{lastReservation}' es inválida.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Ocurrió un error inesperado al generar el número de reserva.", ex);
        }
    }

    private decimal calculate_total_price(decimal nightlyRate, DateTime checkIn, DateTime checkOut)
    {

        if (nightlyRate == 0) return 0;

        //calculate the difference in days
        int days = (checkOut - checkIn).Days;
        if (days <= 0) days = 1; // If the stay is for less than 1 day, a full day's charge applies.

        return days * nightlyRate;
    }

    private bool AreDatesValid(DateTime checkIn, DateTime checkOut, out string errorMessage)
    {
        errorMessage = string.Empty;

        //Check if the check-in date is in the past
        if (checkIn.Date < DateTime.Today)
        {
            errorMessage = "La fecha de check-in no puede ser menor a la actual.";
            return false;
        }

        //Check if check-out is after check-in
        if (checkOut.Date < checkIn.Date)
        {
            errorMessage = "La fecha de check-out no puede estar antes de la fecha de check-in.";
            return false;
        }

        return true;
    }

    private bool IsRoomAvailable(room? room, uint roomId, out string errorMessage)
    {
        errorMessage = string.Empty;

        //Check if the room exists in the database
        if (room == null)
        {
            errorMessage = "La habitación seleccionada no existe.";
            return false;
        }

        // Check if the status is 'Available' (status_id = 1)
        if (room.status_id != 1)
        {
            errorMessage = "La habitación no se encuentra disponible actualmente,";
            return false;
        }

        return true;
    }


    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] booking item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        try
        {
            var existingBooking = await DbContext.Set<booking>().FindAsync(item.id);
            if (existingBooking == null) 
            {
                return NotFound(new { error = true, message = "Reserva no encontrada." });
            }

            uint? releasedRoomId = null;
            var releasedCheckIn = existingBooking.check_in;
            var releasedCheckOut = existingBooking.check_out;

            // Checks if the room is being changed.
            if (existingBooking.room_id != item.room_id)
            { 
                var newRoom = await DbContext.Set<room>().FindAsync(item.room_id);
                if (newRoom == null) return NotFound(new { error = true, message = "La nueva habitación no existe." });
                
                if(IsRoomAvailable(newRoom, item.room_id, out string errorMessage))
                {
                    var oldRoom = await DbContext.Set<room>().FindAsync(existingBooking.room_id);
                    if (oldRoom != null) 
                    {
                        oldRoom.status_id = 1; // Releases the previous room
                        releasedRoomId = oldRoom.id;
                    }
                    
                    newRoom.status_id = 2; // Occupies the new room
                    existingBooking.room_id = item.room_id; 

                    // Updates the reservation's nightly rate with the price of the new room
                    existingBooking.nightly_rate = newRoom.nightly_rate; 
                }
                else
                {
                    return Conflict(new { error = true, message = errorMessage });
                }
            }

            // existingBooking.status_id = item.status_id; 
            
            // Updates the dates
            existingBooking.check_in = item.check_in;
            existingBooking.check_out = item.check_out;

            // Rate update
            // Rate update - Uses the rate the reservation currently has and the updated dates
            existingBooking.total_price = calculate_total_price(existingBooking.nightly_rate, existingBooking.check_in, existingBooking.check_out);

            // Generate alerts if room was released and there are waiting customers
            if (_alertService != null && releasedRoomId.HasValue)
            {
                var notifications = await _alertService.GenerateAlertsForReleasedRoom(
                    releasedRoomId.Value,
                    releasedCheckIn,
                    releasedCheckOut);

                if (notifications.Count > 0)
                {
                    _logger?.LogInformation($"Generated {notifications.Count} alerts when booking {existingBooking.reserve_number} was rescheduled");
                }
            }

            await DbContext.SaveChangesAsync();

            return Ok(new { 
                success = true,
                message = "Reserva actualizada exitosamente. Tu número de reserva es: " + existingBooking.reserve_number,
                data = BuildBookingResponse(existingBooking) 
            });


        }
        catch (Exception ex)
        {
            string realError = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { error = true, message = "Processing error: " + realError });
        }
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromBody] booking item, bool useJson = false)
    {
        if (UseJsonBackend(useJson)) return JsonWriteUnsupported();
        if (DbContext is null) return DbBackendMissing();

        try
        {
            var existingBooking = await DbContext.Set<booking>().FindAsync(item.id);
            if (existingBooking == null) return NotFound(new { error = true, message = "Reserva no encontrada." });

            // Store the old room info for alert generation
            var oldRoom = await DbContext.Set<room>().FindAsync(existingBooking.room_id);

            // Aplica Soft Delete (Cancelamos la reserva)
            existingBooking.status_id = 3;

            // Libera la habitación
            if (oldRoom != null)
            {
                oldRoom.status_id = 1; // 1 = AVAILABLE
            }

            await DbContext.SaveChangesAsync();

            // Generate alerts for waiting queue customers in FIFO order
            if (_alertService != null && oldRoom != null)
            {
                var notifications = await _alertService.GenerateAlertsForReleasedRoom(
                    existingBooking.room_id,
                    existingBooking.check_in,
                    existingBooking.check_out);

                _logger?.LogInformation($"Generated {notifications.Count} alerts when booking {existingBooking.reserve_number} was cancelled");
            }

            return Ok(new { success = true, message = "Reserva cancelada y habitación liberada exitosamente." });
        }
        catch (Exception ex)
        {
            string realError = ex.InnerException?.Message ?? ex.Message;
            return BadRequest(new { error = true, message = "Processing error: " + realError });
        }
    }

    [HttpGet("GetReserveNumberById")]
    public async Task<IActionResult> GetReserveNumberById(uint id, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.id == id)?.reserve_number);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>().Where(b => b.id == id).Select(b => b.reserve_number).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetRoomIdByReserveNumber")]
    public async Task<IActionResult> GetRoomIdByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber)?.room_id);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>().Where(b => b.reserve_number == reserveNumber).Select(b => (uint?)b.room_id).FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetBookingHistoryIdsByReserveNumber")]
    public async Task<IActionResult> GetBookingHistoryIdsByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var bookingIds = JsonContext!.bookings
                .Where(b => b.reserve_number == reserveNumber)
                .Select(b => b.id)
                .ToHashSet();

            return Ok(JsonContext.booking_histories
                .Where(history => bookingIds.Contains(history.booking_id))
                .Select(history => history.id));
        }

        if (DbContext is null) return DbBackendMissing();

        var bookingId = await DbContext.Set<booking>().Where(b => b.reserve_number == reserveNumber).Select(b => (uint?)b.id).FirstOrDefaultAsync();
        if (bookingId is null) return Ok(Array.Empty<uint>());

        var result = await DbContext.Set<booking_history>().Where(h => h.booking_id == bookingId.Value).Select(h => h.id).ToListAsync();
        return Ok(result);
    }

    [HttpGet("GetOverlappingReserveNumbers")]
    public async Task<IActionResult> GetOverlappingReserveNumbers(DateTime startDate, DateTime endDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            var jsonResults = JsonContext!.bookings
                .Where(b => b.check_in < endDate && b.check_out > startDate)
                .Select(b => b.reserve_number);
            return Ok(jsonResults);
        }

        if (DbContext is null) return DbBackendMissing();

        var dbResults = await DbContext.Set<booking>()
            .Where(b => b.check_in < endDate && b.check_out > startDate)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(dbResults);
    }

    [HttpGet("GetReserveNumbersByCheckInDate")]
    public async Task<IActionResult> GetReserveNumbersByCheckInDate(DateTime checkInDate, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings
                .Where(b => b.check_in.Date == checkInDate.Date)
                .Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.check_in.Date == checkInDate.Date)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("GetTotalPriceByReserveNumber")]
    public async Task<IActionResult> GetTotalPriceByReserveNumber(string reserveNumber, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.FirstOrDefault(b => b.reserve_number == reserveNumber)?.total_price);
        }

        if (DbContext is null) return DbBackendMissing();
        var result = await DbContext.Set<booking>()
            .Where(b => b.reserve_number == reserveNumber)
            .Select(b => (decimal?)b.total_price)
            .FirstOrDefaultAsync();
        return Ok(result);
    }

    [HttpGet("GetReserveNumbersByStatusId")]
    public async Task<IActionResult> GetReserveNumbersByStatusId(byte statusId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.Where(b => b.status_id == statusId).Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.status_id == statusId)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("GetReserveNumbersByCustomerId")]
    public async Task<IActionResult> GetReserveNumbersByCustomerId(uint customerId, bool useJson = false)
    {
        if (UseJsonBackend(useJson))
        {
            return Ok(JsonContext!.bookings.Where(b => b.customer_id == customerId).Select(b => b.reserve_number));
        }

        if (DbContext is null) return DbBackendMissing();
        var results = await DbContext.Set<booking>()
            .Where(b => b.customer_id == customerId)
            .Select(b => b.reserve_number)
            .ToListAsync();
        return Ok(results);
    }

    private static object BuildBookingResponse(booking b)
    {
        return new
        {
            b.id, 
            b.reserve_number, 

            b.status_id,
            status_name = b.status?.status_name ?? "Sin estado",
                      
            b.customer_id,
            customer_name = b.customer != null
            ? $"{b.customer.first_name} {b.customer.last_name}"
            : "Sin cliente",

            b.room_id,
            room_number = b.room?.room_number ?? 0,

            b.check_in,
            b.check_out,

            b.nightly_rate,
            b.total_price
        };
    }

    /// <summary>
    /// Attempts to add a booking request to the waiting queue if the room is not available.
    /// This allows customers to be notified when their requested room becomes available.
    /// </summary>
    private async Task<IActionResult> AddToWaitingQueue(SaveBookingDto dto)
    {
        try
        {
            var room = await DbContext!.Set<room>().FindAsync(dto.room_id);
            if (room == null)
                return NotFound(new { error = true, message = "La habitación seleccionada no existe." });

            // Get room category for queue management
            byte roomCategoryId = room.category_id;

            // Queue Status: 1 = Pending
            var queueEntry = new waiting_queue
            {
                customer_id = dto.customer_id,
                room_category_id = roomCategoryId,
                requested_check_in = dto.check_in,
                check_out = dto.check_out,
                status_id = 1, // Pending
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            };

            await DbContext.Set<waiting_queue>().AddAsync(queueEntry);
            await DbContext.SaveChangesAsync();

            _logger?.LogInformation($"Customer {dto.customer_id} added to waiting queue for room category {roomCategoryId}");

            return Conflict(new
            {
                error = true,
                message = "La habitación no está disponible para las fechas solicitadas, pero se ha agregado a la lista de espera.",
                waitingQueueId = queueEntry.id,
                status = "ADDED_TO_QUEUE",
                roomCategoryId = roomCategoryId,
                requestedCheckIn = dto.check_in,
                requestedCheckOut = dto.check_out,
                createdAt = queueEntry.created_at
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding to waiting queue");
            return StatusCode(500, new { error = true, message = "Error al agregar a la lista de espera: " + ex.Message });
        }
    }

    /// <summary>
    /// Retrieves pending alerts (notified customers) for operators to contact them about room availability.
    /// Results are returned in FIFO order (by creation date).
    /// </summary>
    [HttpGet("GetOperatorAlerts")]
    public async Task<IActionResult> GetOperatorAlerts(bool useJson = false)
    {
        if (_alertService == null)
        {
            return StatusCode(503, new { error = true, message = "Alert service not configured." });
        }

        var alerts = await _alertService.GetPendingAlertsForOperator();
        return Ok(alerts);
    }
}
