# Booking Availability Orchestration - Testing & Validation Guide

## Quick Start

### Build the Project
```bash
cd c:\Git\academic-journey\HotelCargaCode
dotnet build
```

### Run All Tests
```bash
# Run all API tests
dotnet test Tests/HotelCarga.ApiModel.Tests/HotelCarga.ApiModel.Tests.csproj

# Run all Web tests
dotnet test Tests/HotelCarga.Web.Tests/HotelCarga.Web.Tests.csproj

# Run both
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test
```bash
dotnet test Tests/HotelCarga.ApiModel.Tests/HotelCarga.ApiModel.Tests.csproj \
  --filter "CreateWithAvailabilityFlow_SelectedRoomAvailable_ReturnsBookingCreated"
```

## Test Organization

### API Integration Tests
**File**: `Tests/HotelCarga.ApiModel.Tests/BookingAvailabilityControllerTests.cs`

Tests the core orchestration logic in `BookingAvailabilityController`.

**Test Classes**:
- `BookingAvailabilityControllerTests` - 11 comprehensive integration tests

**Scenarios Covered**:

| Test Name | Scenario | Expected Result |
|-----------|----------|-----------------|
| SelectedRoomAvailable | User picks AVAILABLE room | BOOKING_CREATED |
| SelectedRoomOccupied + AlternativeExists | User picks OCCUPIED room, alternative in category | ALTERNATIVE_ROOM_SUGGESTED |
| AllRoomsOccupied | All rooms in category occupied | QUEUED_AND_PENDING_BOOKING_CREATED |
| InvalidRequest | Check-out before check-in | VALIDATION_ERROR (400) |
| CustomerNotFound | Non-existent customer_id | VALIDATION_ERROR (404) |
| RoomNotFound | Non-existent room_id | VALIDATION_ERROR (404) |
| PastCheckInDate | Check-in in the past | VALIDATION_ERROR (400) |
| Transaction Integrity | Exception during queue creation | Rollback verified |
| Deterministic Ordering | Multiple alternatives available | First by room_number |

### Web Controller Tests
**File**: `Tests/HotelCarga.Web.Tests/BookingControllerAvailabilityResponseTests.cs`

Tests Web layer response handling and UX branching.

**Test Classes**:
- `BookingControllerAvailabilityResponseTests` - 10 unit tests

**Scenarios Covered**:

| Test Name | Response Type | Web Action | Result |
|-----------|---------------|-----------|---------|
| BookingCreated | BOOKING_CREATED | Redirect to List | Success message |
| AlternativeSuggested | ALTERNATIVE_ROOM_SUGGESTED | Render form with prompt | Show alternative |
| QueuedAndPending | QUEUED_AND_PENDING_BOOKING_CREATED | Redirect to List | Queue message |
| ValidationError | VALIDATION_ERROR | Redisplay form | Error on form |
| ConfirmAlternative_Available | BOOKING_CREATED | Redirect to List | Success |
| ConfirmAlternative_Unavailable | VALIDATION_ERROR | Redirect to Create | Error message |
| AjaxRequest_BookingCreated | BOOKING_CREATED | JSON response | Redirect URL |
| AjaxRequest_AlternativeSuggested | ALTERNATIVE_ROOM_SUGGESTED | JSON response | Alternative data |
| AjaxRequest_QueuedAndPending | QUEUED_AND_PENDING_BOOKING_CREATED | JSON response | Queue data |

## Test Execution Details

### Scenario 1: Booking Created (Happy Path)

```csharp
[Fact]
public async Task CreateWithAvailabilityFlow_SelectedRoomAvailable_ReturnsBookingCreated()
{
    // Arrange
    var request = new BookingAvailabilityRequest(
        customer_id: 2,
        selected_room_id: 1,  // Room 101 (AVAILABLE)
        check_in: new DateTime(2026, 5, 1),
        check_out: new DateTime(2026, 5, 5)
    );

    // Act
    var result = await _controller.CreateWithAvailabilityFlow(request);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(result);
    var response = Assert.IsAssignableFrom<BookingAvailabilityResponse>(createdResult.Value);
    Assert.Equal("BOOKING_CREATED", response.result_type);
    Assert.True(response.success);
    Assert.NotNull(response.booking_created);
    Assert.Equal(1u, response.booking_created.room_id);  // Room 101
    
    // Verify persistence
    var persistedBooking = await _dbContext.Set<booking>()
        .FirstOrDefaultAsync(b => b.reserve_number == response.booking_created.reserve_number);
    Assert.NotNull(persistedBooking);
    Assert.Equal(2, persistedBooking.status_id);  // CONFIRMED
}
```

**Flow**:
1. Setup in-memory DB with 3 rooms in DELUXE category
2. Room 101 is AVAILABLE, no bookings
3. Call API with Room 101
4. API checks availability → AVAILABLE
5. API creates booking with status = CONFIRMED
6. Return 201 Created with BOOKING_CREATED response
7. Verify booking persisted in DB with correct room_id

**Expected Output**:
```
✓ PASS - Booking created with reserve number RES0001
✓ PASS - Room ID set to 101
✓ PASS - Status is CONFIRMED (2)
✓ PASS - Booking persisted to database
```

### Scenario 2: Alternative Room Suggested

```csharp
[Fact]
public async Task CreateWithAvailabilityFlow_SelectedRoomOccupiedAlternativeExists_ReturnsSuggestion()
{
    // Arrange
    var request = new BookingAvailabilityRequest(
        customer_id: 2,
        selected_room_id: 2,  // Room 102 (OCCUPIED)
        check_in: new DateTime(2026, 5, 2),
        check_out: new DateTime(2026, 5, 4)
    );

    // Act
    var result = await _controller.CreateWithAvailabilityFlow(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    var response = Assert.IsType<BookingAvailabilityResponse>(okResult.Value);
    
    Assert.Equal("ALTERNATIVE_ROOM_SUGGESTED", response.result_type);
    Assert.False(response.success);  // Not persisted
    Assert.NotNull(response.alternative_room_suggested);
    Assert.Equal(101u, response.alternative_room_suggested.alternative_room_id);  // Room 101
    
    // Verify NO booking was created
    var bookingCount = await _dbContext.Set<booking>()
        .CountAsync(b => b.customer_id == 2);
    Assert.Equal(0, bookingCount);
}
```

**Flow**:
1. Setup: Room 102 has existing booking 2026-05-01 to 2026-05-05
2. Request booking for Room 102, 2026-05-02 to 2026-05-04
3. API checks Room 102 → OCCUPIED (overlap detected)
4. API searches same category for AVAILABLE rooms
5. Finds Room 101 (no bookings) → First available
6. Return 200 OK with ALTERNATIVE_ROOM_SUGGESTED response
7. Verify NO booking created (user hasn't confirmed yet)

**Expected Output**:
```
✓ PASS - Alternative suggestion returned
✓ PASS - Alternative room is 101
✓ PASS - Response type is ALTERNATIVE_ROOM_SUGGESTED
✓ PASS - No booking created (status check = 0)
```

### Scenario 3: Queue and Pending Booking Created

```csharp
[Fact]
public async Task CreateWithAvailabilityFlow_AllRoomsOccupied_CreatesQueueAndPendingBooking()
{
    // Arrange - Mark all 3 rooms as occupied
    var room101Booking = new booking { /*...occupied...*/ };
    var room103Booking = new booking { /*...occupied...*/ };
    _dbContext.Set<booking>().AddRange(room101Booking, room103Booking);
    await _dbContext.SaveChangesAsync();

    var request = new BookingAvailabilityRequest(
        customer_id: 2,
        selected_room_id: 2,  // Room 102 (also occupied)
        check_in: new DateTime(2026, 5, 2),
        check_out: new DateTime(2026, 5, 4)
    );

    // Act
    var result = await _controller.CreateWithAvailabilityFlow(request);

    // Assert
    var conflictResult = Assert.IsType<ConflictObjectResult>(result);
    var response = Assert.IsType<BookingAvailabilityResponse>(conflictResult.Value);
    
    Assert.Equal("QUEUED_AND_PENDING_BOOKING_CREATED", response.result_type);
    Assert.True(response.success);  // Success but conflict status
    
    // Verify queue entry
    var queueEntry = await _dbContext.Set<waiting_queue>()
        .FirstOrDefaultAsync(w => w.id == response.queued_and_pending.waiting_queue_id);
    Assert.NotNull(queueEntry);
    Assert.Equal(1, queueEntry.status_id);  // PENDING
    
    // Verify pending booking (CRITICAL: room_id is NULL)
    var pendingBooking = await _dbContext.Set<booking>()
        .FirstOrDefaultAsync(b => b.id == response.queued_and_pending.pending_booking_id);
    Assert.NotNull(pendingBooking);
    Assert.Null(pendingBooking.room_id);  // NULL - room not assigned yet
    Assert.Equal(1, pendingBooking.status_id);  // PENDING
}
```

**Flow**:
1. Setup: All 3 rooms have overlapping bookings for 2026-05-02 to 2026-05-04
2. Request booking for Room 102, 2026-05-02 to 2026-05-04
3. API checks Room 102 → OCCUPIED
4. API searches same category → ALL rooms occupied/conflicted
5. Create waiting_queue: status = PENDING, category = DELUXE
6. Create booking: room_id = NULL, status = PENDING
7. Return 409 Conflict with QUEUED_AND_PENDING_BOOKING_CREATED response
8. Verify both entries persisted with correct status values

**Expected Output**:
```
✓ PASS - Waiting queue created with ID 1
✓ PASS - Queue status is PENDING
✓ PASS - Pending booking created with ID 1
✓ PASS - Booking room_id is NULL (not assigned)
✓ PASS - Booking status is PENDING
✓ PASS - Response type is QUEUED_AND_PENDING_BOOKING_CREATED
```

## Running Tests in CI/CD

### GitHub Actions Example
```yaml
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --logger "trx" --collect:"XPlat Code Coverage"
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
```

## Test Statistics

**Total Tests**: 21
- **API Integration Tests**: 11
- **Web Unit Tests**: 10

**Coverage Targets**:
- BookingAvailabilityController: 95%+
- BookingController orchestration methods: 90%+
- Response handling logic: 100%

**Estimated Run Time**:
- API tests: ~2-3 seconds
- Web tests: ~1-2 seconds
- Total: ~5 seconds

## Debugging Tests

### Run Single Test with Breakpoint
```bash
dotnet test Tests/HotelCarga.ApiModel.Tests/HotelCarga.ApiModel.Tests.csproj \
  --filter "CreateWithAvailabilityFlow_SelectedRoomAvailable_ReturnsBookingCreated" \
  --logger "console;verbosity=detailed"
```

### View Detailed Error Output
```bash
dotnet test --logger "console;verbosity=detailed" 2>&1 | tee test_output.log
```

### Test in Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Find test in list
3. Right-click → Debug Selected Tests
4. Set breakpoints in test or code

## Validation Checklist

Before deploying to production:

- [ ] All 21 tests pass locally
- [ ] All 21 tests pass in CI/CD
- [ ] Code coverage > 90%
- [ ] No compiler warnings
- [ ] Database migration tested
- [ ] Booking creation flow works end-to-end
- [ ] Alternative suggestion works end-to-end
- [ ] Queue creation works end-to-end
- [ ] Web form displays correctly with responses
- [ ] AJAX requests return correct JSON
- [ ] Error messages clear and localized

## Known Limitations

1. **In-Memory Database**: Tests use InMemory DB for speed. Real DB trigger behavior not tested.
   - *Mitigation*: Add separate integration tests against MySQL if needed

2. **Mock HTTP Context**: Web tests mock HTTP context.
   - *Mitigation*: Run manual smoke tests in staging

3. **No Concurrency Testing**: Tests don't simulate concurrent bookings.
   - *Mitigation*: Monitor production for race condition errors

## Support

See MIGRATION_GUIDE.md for deployment and operational details.

---

**Test Framework**: xUnit 2.7.0  
**Database**: InMemory / MySQL 8.4  
**Language**: C# 12 / .NET 10.0  
**Date**: 2026-04-19
