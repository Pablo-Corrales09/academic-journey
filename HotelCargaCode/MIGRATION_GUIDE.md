# Booking Availability Orchestration - Implementation Guide

## Overview

This document describes the implementation of the booking availability orchestration system that replaces the previous disconnected booking/waiting queue logic with a unified, transactional business flow.

## What Changed

### 1. Database Schema Changes

**Migration**: `/sql/migrations/001_allow_nullable_booking_room.sql`

- **Modified**: `booking.room_id` is now **nullable** (`INT UNSIGNED NULL`)
- **Reason**: Allows PENDING bookings to exist without a room assignment, supporting deferred room allocation
- **Foreign Key**: Updated to use `ON DELETE SET NULL` behavior

**How to apply**:
```sql
-- Run migration script directly
mysql -u<user> -p<password> hotelcarga < sql/migrations/001_allow_nullable_booking_room.sql
```

### 2. Entity Model Changes

**File**: `HotelCarga.DbModel/Entities/Booking.cs`

- `room_id` changed from `uint` to `uint?` (nullable)
- Added documentation comment explaining PENDING booking semantics

**File**: `HotelCarga.DbModel/HotelCargaContext.cs`

- Updated FK configuration for booking-room relationship
- Changed from `OnDelete(DeleteBehavior.ClientSetNull)` to `OnDelete(DeleteBehavior.SetNull)`
- Added `.IsRequired(false)` to make FK optional

### 3. New API Contracts

**Files**:
- `HotelCarga.ApiModel/Contracts/BookingAvailabilityRequest.cs`
- `HotelCarga.ApiModel/Contracts/BookingAvailabilityResponse.cs`

**Request Contract** (`BookingAvailabilityRequest`):
```csharp
public record BookingAvailabilityRequest(
    uint customer_id,
    uint selected_room_id,
    DateTime check_in,
    DateTime check_out,
    bool useJson = false);
```

**Response Contract** (`BookingAvailabilityResponse`):
- Unified response with `result_type` discriminator
- Result types:
  - `BOOKING_CREATED`: Direct booking successful
  - `ALTERNATIVE_ROOM_SUGGESTED`: Alternative room available
  - `QUEUED_AND_PENDING_BOOKING_CREATED`: All rooms occupied, added to queue
  - `VALIDATION_ERROR`: Input validation failure

### 4. New API Orchestration Endpoint

**File**: `HotelCarga.ApiModel/Controllers/BookingAvailabilityController.cs`

**Endpoints**:

#### `POST /BookingAvailability/CreateWithAvailabilityFlow`
Main orchestration endpoint implementing the three-branch business logic:

1. **If selected room is AVAILABLE and not out of order**:
   - Create booking immediately with status = CONFIRMED
   - Return `BOOKING_CREATED` response

2. **If selected room is OCCUPIED/unavailable**:
   - Search for first AVAILABLE room in same category with no booking conflicts
   - If found: Return `ALTERNATIVE_ROOM_SUGGESTED` response (no booking created)
   - If not found: Proceed to step 3

3. **If all rooms in category are occupied**:
   - Create `waiting_queue` entry with status_id = PENDING
   - Create `booking` entry with room_id = NULL, status_id = PENDING
   - Return `QUEUED_AND_PENDING_BOOKING_CREATED` response

#### `POST /BookingAvailability/ConfirmAlternativeRoom`
Confirms user acceptance of suggested alternative room.

**Request**:
```csharp
public record ConfirmAlternativeRoomRequest(
    uint customer_id,
    uint alternative_room_id,
    DateTime check_in,
    DateTime check_out);
```

### 5. Web Service Updates

**File**: `HotelCarga.Web/Services/BookingApiService.cs`

Added:
- `BookingAvailabilityRequestDto` record
- `BookingAvailabilityResponseDto` class
- `CreateWithAvailabilityFlowAsync()` method

### 6. Web Controller Response Handling

**File**: `HotelCarga.Web/Controllers/BookingController.cs`

**Updated Create POST action** to call new orchestration endpoint and branch based on result:

- `HandleBookingCreatedResponse()`: Redirect to List with success message
- `HandleAlternativeRoomResponse()`: Display form with alternative room prompt
- `HandleQueuedAndPendingResponse()`: Redirect to List with queued message
- `HandleValidationErrorResponse()`: Redisplay form with error

**New action**: `ConfirmAlternativeRoom()` - Accepts alternative room suggestion and creates booking

### 7. View Model Updates

**File**: `HotelCarga.Web/Models/Bookings/BookingViewModels.cs`

Added to `BookingFormViewModel`:
```csharp
[ValidateNever]
public string? AlternativeRoomData { get; set; }
```

## Business Logic Flow

### Scenario 1: Selected Room Available
```
User selects Room 101 (AVAILABLE), dates 2026-05-01 to 2026-05-05
↓
API checks: Room 101 status, no overlapping bookings
↓
Result: AVAILABLE
↓
API creates booking immediately (status = CONFIRMED)
↓
Response: BOOKING_CREATED with booking details
↓
Web redirects to booking list with success message
```

### Scenario 2: Selected Room Occupied, Alternative Available
```
User selects Room 102 (OCCUPIED), dates 2026-05-02 to 2026-05-04
↓
API checks: Room 102 occupied/overlapped
↓
API searches same category (DELUXE) for AVAILABLE rooms:
  - Room 101: AVAILABLE, no conflicts ✓
  - Return Room 101 as alternative
↓
Response: ALTERNATIVE_ROOM_SUGGESTED with Room 101 details
↓
Web displays prompt: "Room 102 not available, accept Room 101?"
↓
User confirms → POST to ConfirmAlternativeRoom
↓
API creates booking for Room 101 (status = CONFIRMED)
↓
Web redirects with success message
```

### Scenario 3: All Rooms Occupied
```
User selects Room 102 (OCCUPIED), dates overlap with all rooms in category
↓
API checks: All 3 DELUXE rooms have conflicts during requested dates
↓
API creates:
  1. waiting_queue entry: status_id = PENDING, room_category_id = 1
  2. booking entry: room_id = NULL, status_id = PENDING
↓
Response: QUEUED_AND_PENDING_BOOKING_CREATED with queue + booking details
↓
Web displays success: "Added to waiting list. Request: RQ-12345678"
↓
Customer notified when room available (future: AlertService processes queue)
```

## Testing

### Running Tests

**API Integration Tests** (tests orchestration logic):
```bash
cd Tests/HotelCarga.ApiModel.Tests
dotnet test
```

**Key tests**:
- `CreateWithAvailabilityFlow_SelectedRoomAvailable_ReturnsBookingCreated`: Scenario 1
- `CreateWithAvailabilityFlow_SelectedRoomOccupiedAlternativeExists_ReturnsSuggestion`: Scenario 2
- `CreateWithAvailabilityFlow_AllRoomsOccupied_CreatesQueueAndPendingBooking`: Scenario 3
- `CreateWithAvailabilityFlow_InvalidRequest_ReturnsBadRequest`: Validation
- `CreateWithAvailabilityFlow_CustomerNotFound_ReturnsNotFound`: Error handling
- `CreateWithAvailabilityFlow_Transaction_RolledBackOnException`: Transactional integrity

**Web Controller Tests** (tests response handling):
```bash
cd Tests/HotelCarga.Web.Tests
dotnet test
```

**Key tests**:
- `Create_BookingCreatedResponse_ReturnsSuccessRedirect`
- `Create_AlternativeRoomSuggestedResponse_ReturnsViewWithPrompt`
- `Create_QueuedAndPendingResponse_ReturnsSuccessWithQueuedMessage`
- `Create_ValidationErrorResponse_RedisplayFormWithError`
- `Create_AjaxRequest_*_ReturnsJson*`

### Test Coverage

- **Positive paths**: All three business scenarios
- **Negative paths**: Invalid dates, missing entities, conflicts
- **Edge cases**: Deterministic ordering, transaction rollback
- **Integration**: End-to-end request/response flow

## Deployment Checklist

### Pre-Deployment
- [ ] Back up production database
- [ ] Review all migration scripts
- [ ] Verify test suite passes locally
- [ ] Test in staging environment

### Deployment Steps

1. **Stop APIs**:
   ```bash
   # Stop HotelCarga.ApiModel
   # Stop HotelCarga.Web
   ```

2. **Apply database migration**:
   ```sql
   mysql -u<user> -p<password> hotelcarga < sql/migrations/001_allow_nullable_booking_room.sql
   ```

3. **Deploy code**:
   - Deploy `HotelCarga.DbModel` (entity/context changes)
   - Deploy `HotelCarga.ApiModel` (new controller)
   - Deploy `HotelCarga.Web` (updated service/controller)

4. **Start APIs**:
   ```bash
   # Start HotelCarga.ApiModel
   # Start HotelCarga.Web
   ```

5. **Smoke Tests**:
   - Create booking with available room → BOOKING_CREATED
   - Create booking with occupied room + alternative → ALTERNATIVE_ROOM_SUGGESTED
   - Create booking with all occupied → QUEUED_AND_PENDING_BOOKING_CREATED
   - Verify queue entries in database

### Post-Deployment
- [ ] Monitor error logs
- [ ] Verify booking creation flows work
- [ ] Check waiting queue entries are created correctly
- [ ] Validate pending bookings have NULL room_id
- [ ] Run full test suite in production (read-only)

## API Examples

### Example 1: Create Booking (Available Room)

**Request**:
```http
POST /BookingAvailability/CreateWithAvailabilityFlow
Content-Type: application/json

{
  "customer_id": 1,
  "selected_room_id": 101,
  "check_in": "2026-05-01T00:00:00",
  "check_out": "2026-05-05T00:00:00"
}
```

**Response (201 Created)**:
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
    "customer_id": 1,
    "room_id": 101,
    "room_number": 101,
    "check_in": "2026-05-01T00:00:00",
    "check_out": "2026-05-05T00:00:00",
    "nightly_rate": 150.00,
    "total_price": 600.00
  }
}
```

### Example 2: Create Booking (Alternative Room Available)

**Request**:
```http
POST /BookingAvailability/CreateWithAvailabilityFlow
Content-Type: application/json

{
  "customer_id": 1,
  "selected_room_id": 102,
  "check_in": "2026-05-02T00:00:00",
  "check_out": "2026-05-04T00:00:00"
}
```

**Response (200 OK)**:
```json
{
  "result_type": "ALTERNATIVE_ROOM_SUGGESTED",
  "success": false,
  "message": "La habitación seleccionada no está disponible, pero encontramos una alternativa.",
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
  }
}
```

### Example 3: Create Booking (All Occupied - Queue)

**Request**:
```http
POST /BookingAvailability/CreateWithAvailabilityFlow
Content-Type: application/json

{
  "customer_id": 1,
  "selected_room_id": 102,
  "check_in": "2026-05-01T00:00:00",
  "check_out": "2026-05-05T00:00:00"
}
```

**Response (409 Conflict)**:
```json
{
  "result_type": "QUEUED_AND_PENDING_BOOKING_CREATED",
  "success": true,
  "message": "Todas las habitaciones en esta categoría están ocupadas. Se ha registrado su solicitud.",
  "queued_and_pending": {
    "waiting_queue_id": 1,
    "request_number": "RQ-A1B2C3D4",
    "pending_booking_id": 1,
    "pending_reserve_number": "RES0001",
    "room_category_name": "DELUXE",
    "requested_check_in": "2026-05-01T00:00:00",
    "requested_check_out": "2026-05-05T00:00:00",
    "queue_status_name": "PENDING",
    "created_at": "2026-04-19T10:30:00"
  }
}
```

## Troubleshooting

### Issue: "Room is already booked for this period" - Trigger Error

**Cause**: Database trigger `trg_prevent_double_booking` is firing despite pre-checks

**Solution**:
1. Verify selected room status before API call
2. Check for race conditions (concurrent bookings)
3. Implement retry logic in Web service with exponential backoff

### Issue: Pending Booking Not Found After Queue Creation

**Cause**: Room assignment occurred before API returned response

**Solution**:
1. Verify pending booking exists with NULL room_id
2. Check booking_history for status changes
3. Wait 5-10 seconds and query again (async processing)

### Issue: Alternative Room No Longer Available When Confirming

**Cause**: Another user booked the alternative room while user was reviewing

**Solution**:
1. API detects this and returns VALIDATION_ERROR
2. Web controller redirects user with message "Room no longer available"
3. User can restart booking flow with different dates/room

## Future Enhancements

1. **Asynchronous Queue Processing**:
   - Implement background job to process waiting queue
   - Auto-create bookings when rooms become available
   - Send notifications to queued customers

2. **Queue Priority Levels**:
   - Implement priority based on booking value or loyalty
   - Prioritize earlier check-in dates

3. **Alternative Room Auto-Accept**:
   - Add preference setting: auto-accept alternatives with same price
   - Skip confirmation step for eligible customers

4. **Analytics**:
   - Track alternative acceptance rate
   - Monitor queue processing time
   - Identify peak occupancy periods

## Support

For issues or questions:
1. Check test cases for expected behavior
2. Review API response contracts
3. Verify database state after operations
4. Check application logs for detailed error messages

---

**Last Updated**: 2026-04-19  
**Version**: 1.0  
**Status**: Ready for Production
