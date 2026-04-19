using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using HotelCarga.Web.Controllers;
using HotelCarga.Web.Services;
using HotelCarga.Models.Bookings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace HotelCarga.Web.Tests;

/// <summary>
/// Unit tests for BookingController's response handling to structured availability responses.
/// Verifies that the controller correctly branches based on result_type and provides appropriate UX feedback.
/// </summary>
public class BookingControllerAvailabilityResponseTests
{
    private readonly Mock<IBookingApiService> _mockBookingService;
    private readonly Mock<ICustomerApiService> _mockCustomerService;
    private readonly Mock<IRoomApiService> _mockRoomService;
    private readonly BookingController _controller;

    public BookingControllerAvailabilityResponseTests()
    {
        _mockBookingService = new Mock<IBookingApiService>();
        _mockCustomerService = new Mock<ICustomerApiService>();
        _mockRoomService = new Mock<IRoomApiService>();

        // Setup mock HTTP context for AJAX detection
        var httpContext = new DefaultHttpContext();
        var controllerContext = new ControllerContext { HttpContext = httpContext };

        _controller = new BookingController(
            _mockBookingService.Object,
            _mockCustomerService.Object,
            _mockRoomService.Object,
            Mock.Of<IHttpClientFactory>(),
            Mock.Of<IOptions<ApiSettings>>(o => o.Value == new ApiSettings { BaseUrl = "http://api" }))
        {
            ControllerContext = controllerContext
        };
    }

    [Fact]
    public async Task Create_BookingCreatedResponse_ReturnsSuccessRedirect()
    {
        // Arrange
        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 101,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "BOOKING_CREATED",
            success = true,
            message = "Reserva creada exitosamente. Tu número de reserva es: RES0001",
            booking_created = new { id = 1, reserve_number = "RES0001", status_id = 2, room_id = 101 }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingController.List), redirectResult.ActionName);
        Assert.Equal("Reserva creada exitosamente. Tu número de reserva es: RES0001", _controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task Create_AlternativeRoomSuggestedResponse_ReturnsViewWithPrompt()
    {
        // Arrange
        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 102,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "ALTERNATIVE_ROOM_SUGGESTED",
            success = false,
            message = "La habitación seleccionada no está disponible, pero encontramos una alternativa.",
            alternative_room_suggested = new
            {
                original_requested_room_id = 102,
                alternative_room_id = 103,
                alternative_room_number = 103,
                room_category_name = "DELUXE"
            }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<BookingFormViewModel>(viewResult.Model);
        Assert.True(returnedModel.ShowNoAvailabilityPrompt);
        Assert.Contains("alternativa", returnedModel.NoAvailabilityPromptText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_QueuedAndPendingResponse_ReturnsSuccessWithQueuedMessage()
    {
        // Arrange
        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 102,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "QUEUED_AND_PENDING_BOOKING_CREATED",
            success = true,
            message = "Todas las habitaciones en esta categoría están ocupadas.",
            queued_and_pending = new
            {
                waiting_queue_id = 1,
                request_number = "RQ-12345678",
                pending_booking_id = 1,
                pending_reserve_number = "RES0001",
                room_category_name = "DELUXE"
            }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingController.List), redirectResult.ActionName);
        Assert.NotNull(_controller.TempData["SuccessMessage"]);
        Assert.NotNull(_controller.TempData["QueuedMessage"]);
    }

    [Fact]
    public async Task Create_ValidationErrorResponse_RedisplayFormWithError()
    {
        // Arrange
        var model = new BookingFormViewModel
        {
            CustomerId = 999, // Non-existent
            RoomId = 101,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "VALIDATION_ERROR",
            success = false,
            message = "Customer not found.",
            validation_error = new { error_code = "CUSTOMER_NOT_FOUND" }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(viewResult.ViewData.ModelState.IsValid);
        Assert.True(viewResult.ViewData.ModelState.ContainsKey(string.Empty));
    }

    [Fact]
    public async Task ConfirmAlternativeRoom_AlternativeStillAvailable_CreatesBooking()
    {
        // Arrange
        var response = new BookingAvailabilityResponseDto
        {
            result_type = "BOOKING_CREATED",
            success = true,
            message = "Habitación alternativa reservada exitosamente.",
            booking_created = new { id = 1, reserve_number = "RES0001", room_id = 103 }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmAlternativeRoom(
            customerId: 1,
            alternativeRoomId: 103,
            checkIn: new DateTime(2026, 5, 1),
            checkOut: new DateTime(2026, 5, 5));

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingController.List), redirectResult.ActionName);
    }

    [Fact]
    public async Task ConfirmAlternativeRoom_AlternativeNoLongerAvailable_ReturnConflictMessage()
    {
        // Arrange
        var response = new BookingAvailabilityResponseDto
        {
            result_type = "VALIDATION_ERROR",
            success = false,
            message = "La habitación alternativa ya no está disponible.",
            validation_error = new { error_code = "ROOM_NO_LONGER_AVAILABLE" }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmAlternativeRoom(
            customerId: 1,
            alternativeRoomId: 103,
            checkIn: new DateTime(2026, 5, 1),
            checkOut: new DateTime(2026, 5, 5));

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BookingController.Create), redirectResult.ActionName);
        Assert.NotNull(_controller.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Create_AjaxRequest_BookingCreated_ReturnsJsonWithRedirect()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 101,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "BOOKING_CREATED",
            success = true,
            message = "Reserva creada exitosamente."
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task Create_AjaxRequest_AlternativeAvailable_ReturnsJsonWithAlternativeData()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 102,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "ALTERNATIVE_ROOM_SUGGESTED",
            success = false,
            message = "Alternative available",
            alternative_room_suggested = new { alternative_room_id = 103 }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public async Task Create_AjaxRequest_QueuedAndPending_ReturnsJsonWithQueueData()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var model = new BookingFormViewModel
        {
            CustomerId = 1,
            RoomId = 102,
            CheckIn = new DateTime(2026, 5, 1),
            CheckOut = new DateTime(2026, 5, 5)
        };

        var response = new BookingAvailabilityResponseDto
        {
            result_type = "QUEUED_AND_PENDING_BOOKING_CREATED",
            success = true,
            message = "Added to queue",
            queued_and_pending = new { waiting_queue_id = 1, request_number = "RQ-12345" }
        };

        _mockBookingService
            .Setup(s => s.CreateWithAvailabilityFlowAsync(It.IsAny<BookingAvailabilityRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(model, false);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }
}
