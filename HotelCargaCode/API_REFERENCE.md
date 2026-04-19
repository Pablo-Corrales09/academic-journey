# API Reference - Booking Availability Orchestration

## Endpoints

### POST /BookingAvailability/CreateWithAvailabilityFlow

Creates a booking with availability-aware logic. Automatically handles three scenarios:
1. Room available → Create booking
2. Room unavailable + alternative exists → Suggest alternative
3. All rooms unavailable → Create pending booking + queue entry

**Request**:
```http
POST /BookingAvailability/CreateWithAvailabilityFlow HTTP/1.1
Content-Type: application/json

{
  "customer_id": 2,
  "selected_room_id": 101,
  "check_in": "2026-05-01T00:00:00Z",
  "check_out": "2026-05-05T00:00:00Z"
}
```

**Request Body** (C# Record):
```csharp
public record BookingAvailabilityRequest(
    uint customer_id,
    uint selected_room_id,
    DateTime check_in,
    DateTime check_out,
    bool useJson = false);
```

**Validation**:
- ✓ `customer_id` must be > 0
- ✓ `selected_room_id` must be > 0
- ✓ `check_in` must be >= today (not in past)
- ✓ `check_out` must be > `check_in`

**Responses**:

#### 201 Created - BOOKING_CREATED
Room was available, booking created immediately.

```json
{
  "result_type": "BOOKING_CREATED",
  "success": true,
  "message": "Reserva creada exitosamente. Tu número de reserva es: RES0001",
  "booking_created": {
    "id": 1,
    "reserve_number": "RES0001",
    "status_id": 2,
    "status_name": "CONFIRMED",
    "customer_id": 2,
    "room_id": 101,
    "room_number": 101,
    "check_in": "2026-05-01T00:00:00Z",
    "check_out": "2026-05-05T00:00:00Z",
    "nightly_rate": 150.00,
    "total_price": 600.00,
    "created_at": "2026-04-19T10:30:00Z"
  },
  "alternative_room_suggested": null,
  "queued_and_pending": null,
  "validation_error": null
}
```

#### 200 OK - ALTERNATIVE_ROOM_SUGGESTED
Selected room unavailable, but alternative exists. No booking created yet.

```json
{
  "result_type": "ALTERNATIVE_ROOM_SUGGESTED",
  "success": false,
  "message": "La habitación seleccionada no está disponible, pero encontramos una alternativa en la misma categoría.",
  "booking_created": null,
  "alternative_room_suggested": {
    "original_requested_room_id": 102,
    "original_requested_room_number": 102,
    "room_category_name": "DELUXE",
    "alternative_room_id": 101,
    "alternative_room_number": 101,
    "alternative_room_status_id": 1,
    "alternative_room_status_name": "AVAILABLE",
    "alternative_nightly_rate": 150.00,
    "floor_number": 1
  },
  "queued_and_pending": null,
  "validation_error": null
}
```

#### 409 Conflict - QUEUED_AND_PENDING_BOOKING_CREATED
All rooms in category occupied. Customer added to waiting queue.

```json
{
  "result_type": "QUEUED_AND_PENDING_BOOKING_CREATED",
  "success": true,
  "message": "Todas las habitaciones en esta categoría están ocupadas. Se ha registrado su solicitud en la lista de espera.",
  "booking_created": null,
  "alternative_room_suggested": null,
  "queued_and_pending": {
    "waiting_queue_id": 1,
    "request_number": "RQ-A1B2C3D4",
    "pending_booking_id": 1,
    "pending_reserve_number": "RES0001",
    "room_category_name": "DELUXE",
    "requested_check_in": "2026-05-01T00:00:00Z",
    "requested_check_out": "2026-05-05T00:00:00Z",
    "queue_status_name": "PENDING",
    "created_at": "2026-04-19T10:30:00Z"
  },
  "validation_error": null
}
```

#### 400 Bad Request - VALIDATION_ERROR
Invalid input parameters.

```json
{
  "result_type": "VALIDATION_ERROR",
  "success": false,
  "message": "Check-out date must be after check-in date.",
  "booking_created": null,
  "alternative_room_suggested": null,
  "queued_and_pending": null,
  "validation_error": {
    "error_code": "INVALID_REQUEST",
    "error_message": "Check-out date must be after check-in date.",
    "field_errors": null
  }
}
```

#### 404 Not Found - VALIDATION_ERROR
Customer or room not found.

```json
{
  "result_type": "VALIDATION_ERROR",
  "success": false,
  "message": "Customer not found.",
  "booking_created": null,
  "alternative_room_suggested": null,
  "queued_and_pending": null,
  "validation_error": {
    "error_code": "CUSTOMER_NOT_FOUND",
    "error_message": "El cliente seleccionado no existe.",
    "field_errors": null
  }
}
```

#### 500 Internal Server Error
Server error during processing.

```json
{
  "result_type": "VALIDATION_ERROR",
  "success": false,
  "message": "Processing error: Invalid queue status configuration",
  "booking_created": null,
  "alternative_room_suggested": null,
  "queued_and_pending": null,
  "validation_error": {
    "error_code": "PROCESSING_ERROR",
    "error_message": "Invalid queue status configuration"
  }
}
```

---

### POST /BookingAvailability/ConfirmAlternativeRoom

Confirms user's acceptance of suggested alternative room. Creates booking with alternative room.

**Request**:
```http
POST /BookingAvailability/ConfirmAlternativeRoom HTTP/1.1
Content-Type: application/json

{
  "customer_id": 2,
  "alternative_room_id": 101,
  "check_in": "2026-05-01T00:00:00Z",
  "check_out": "2026-05-05T00:00:00Z"
}
```

**Request Body** (C# Record):
```csharp
public record ConfirmAlternativeRoomRequest(
    uint customer_id,
    uint alternative_room_id,
    DateTime check_in,
    DateTime check_out);
```

**Response**:
Same as CreateWithAvailabilityFlow. Returns:
- **201 Created** if alternative still available → `BOOKING_CREATED`
- **409 Conflict** if alternative booked in interim → `VALIDATION_ERROR`

---

## Response Contracts

### BookingAvailabilityResponse
```csharp
public class BookingAvailabilityResponse
{
    public string result_type { get; set; }              // BOOKING_CREATED | ALTERNATIVE_ROOM_SUGGESTED | QUEUED_AND_PENDING_BOOKING_CREATED | VALIDATION_ERROR
    public bool success { get; set; }                    // true if operation successful
    public string message { get; set; }                  // User-friendly message
    public BookingDataPayload? booking_created { get; set; }
    public AlternativeRoomPayload? alternative_room_suggested { get; set; }
    public QueuedAndPendingPayload? queued_and_pending { get; set; }
    public ValidationErrorPayload? validation_error { get; set; }
}
```

### BookingDataPayload
```csharp
public class BookingDataPayload
{
    public uint id { get; set; }
    public string? reserve_number { get; set; }
    public byte status_id { get; set; }                 // 2 = CONFIRMED
    public string status_name { get; set; }             // "CONFIRMED"
    public uint customer_id { get; set; }
    public uint room_id { get; set; }
    public uint room_number { get; set; }
    public DateTime check_in { get; set; }
    public DateTime check_out { get; set; }
    public decimal nightly_rate { get; set; }
    public decimal total_price { get; set; }
    public DateTime? created_at { get; set; }
}
```

### AlternativeRoomPayload
```csharp
public class AlternativeRoomPayload
{
    public uint original_requested_room_id { get; set; }
    public uint original_requested_room_number { get; set; }
    public string room_category_name { get; set; }
    public uint alternative_room_id { get; set; }
    public uint alternative_room_number { get; set; }
    public byte alternative_room_status_id { get; set; }
    public string alternative_room_status_name { get; set; }
    public decimal alternative_nightly_rate { get; set; }
    public byte floor_number { get; set; }
}
```

### QueuedAndPendingPayload
```csharp
public class QueuedAndPendingPayload
{
    public uint waiting_queue_id { get; set; }
    public string request_number { get; set; }          // RQ-XXXXXXXX format
    public uint pending_booking_id { get; set; }
    public string? pending_reserve_number { get; set; }
    public string room_category_name { get; set; }
    public DateTime requested_check_in { get; set; }
    public DateTime requested_check_out { get; set; }
    public string queue_status_name { get; set; }       // "PENDING"
    public DateTime? created_at { get; set; }
}
```

### ValidationErrorPayload
```csharp
public class ValidationErrorPayload
{
    public string error_code { get; set; }
    public string error_message { get; set; }
    public Dictionary<string, string>? field_errors { get; set; }
}
```

---

## Result Type Decisions

| Result Type | HTTP Status | Booking Created | Queue Created | User Action |
|-------------|-------------|-----------------|---------------|------------|
| BOOKING_CREATED | 201 | ✓ Yes | ✗ No | Done |
| ALTERNATIVE_ROOM_SUGGESTED | 200 | ✗ No | ✗ No | Confirm alternative or retry |
| QUEUED_AND_PENDING_BOOKING_CREATED | 409 | ✓ Yes (pending) | ✓ Yes | Wait for notification |
| VALIDATION_ERROR | 400/404 | ✗ No | ✗ No | Fix inputs and retry |

---

## Status ID Reference

### Booking Status
- `1` = PENDING (no room assigned yet)
- `2` = CONFIRMED (room assigned, active booking)
- `3` = CANCELED (soft deleted)

### Queue Status
- `1` = PENDING (waiting for room)
- `2` = CONFIRMED (room assigned)
- `3` = CANCELED (user canceled)
- `4` = DISCARDED (dates passed)

### Room Status
- `1` = AVAILABLE (can be booked)
- `2` = OCCUPIED (currently booked)
- `3` = OUT OF ORDER (maintenance/unavailable)

---

## Request Examples

### Using cURL

**Scenario 1: Room Available**
```bash
curl -X POST http://localhost:5000/BookingAvailability/CreateWithAvailabilityFlow \
  -H "Content-Type: application/json" \
  -d '{
    "customer_id": 2,
    "selected_room_id": 101,
    "check_in": "2026-05-01T00:00:00Z",
    "check_out": "2026-05-05T00:00:00Z"
  }'
```

**Scenario 2: Alternative Available**
```bash
curl -X POST http://localhost:5000/BookingAvailability/CreateWithAvailabilityFlow \
  -H "Content-Type: application/json" \
  -d '{
    "customer_id": 2,
    "selected_room_id": 102,
    "check_in": "2026-05-02T00:00:00Z",
    "check_out": "2026-05-04T00:00:00Z"
  }'
```

**Scenario 3: Confirm Alternative**
```bash
curl -X POST http://localhost:5000/BookingAvailability/ConfirmAlternativeRoom \
  -H "Content-Type: application/json" \
  -d '{
    "customer_id": 2,
    "alternative_room_id": 101,
    "check_in": "2026-05-02T00:00:00Z",
    "check_out": "2026-05-04T00:00:00Z"
  }'
```

### Using C# HttpClient

```csharp
var request = new BookingAvailabilityRequest(
    customer_id: 2,
    selected_room_id: 101,
    check_in: new DateTime(2026, 5, 1),
    check_out: new DateTime(2026, 5, 5));

var response = await httpClient.PostAsJsonAsync(
    "http://localhost:5000/BookingAvailability/CreateWithAvailabilityFlow",
    request);

var result = await response.Content.ReadAsAsync<BookingAvailabilityResponse>();

switch (result.result_type)
{
    case "BOOKING_CREATED":
        Console.WriteLine($"Booked: {result.booking_created.reserve_number}");
        break;
    case "ALTERNATIVE_ROOM_SUGGESTED":
        Console.WriteLine($"Alternative: Room {result.alternative_room_suggested.alternative_room_number}");
        break;
    case "QUEUED_AND_PENDING_BOOKING_CREATED":
        Console.WriteLine($"Queued: {result.queued_and_pending.request_number}");
        break;
}
```

---

## Error Handling

### Recommended Client-Side Logic

```javascript
async function createBooking(customerId, roomId, checkIn, checkOut) {
    try {
        const response = await fetch('/BookingAvailability/CreateWithAvailabilityFlow', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                customer_id: customerId,
                selected_room_id: roomId,
                check_in: checkIn,
                check_out: checkOut
            })
        });

        const result = await response.json();

        switch (result.result_type) {
            case 'BOOKING_CREATED':
                showSuccess(`Booking created: ${result.booking_created.reserve_number}`);
                redirectToBookingList();
                break;

            case 'ALTERNATIVE_ROOM_SUGGESTED':
                showAlternativePrompt(result.alternative_room_suggested);
                break;

            case 'QUEUED_AND_PENDING_BOOKING_CREATED':
                showSuccess(`Added to queue: ${result.queued_and_pending.request_number}`);
                redirectToBookingList();
                break;

            case 'VALIDATION_ERROR':
                showError(result.validation_error.error_message);
                break;
        }
    } catch (error) {
        showError(`Request failed: ${error.message}`);
    }
}

function showAlternativePrompt(alternative) {
    const message = `Room ${alternative.original_requested_room_number} is not available. ` +
                   `Room ${alternative.alternative_room_number} available at $${alternative.alternative_nightly_rate}/night?`;
    
    if (confirm(message)) {
        confirmAlternativeRoom(alternative.alternative_room_id);
    }
}
```

---

## Rate Limiting

Currently not implemented. Recommend adding:
- 10 requests per second per customer_id
- 100 requests per second per IP
- Return 429 Too Many Requests if exceeded

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-04-19 | Initial release |

---

**Last Updated**: 2026-04-19  
**Stability**: Production Ready  
**Support**: See MIGRATION_GUIDE.md and TESTING_GUIDE.md
