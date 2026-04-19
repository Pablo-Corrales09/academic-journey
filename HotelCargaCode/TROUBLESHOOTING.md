# Booking Creation Troubleshooting Guide

## Build Status: ✅ SUCCESS

Compilation is successful (0 errors, 0 warnings).

## Diagnostic Steps

### Step 1: Verify API is Running

**Critical:** The Web layer calls the API at `/BookingAvailability/CreateWithAvailabilityFlow`

Check if API is running:
```bash
# In a terminal, check if the API process is running
Get-Process | Where-Object {$_.ProcessName -match "dotnet"}

# Or check the port is listening
netstat -ano | findstr :5000
netstat -ano | findstr :5001
```

**Expected**: HotelCarga.ApiModel should be running on port 5000 (HTTP) or 5001 (HTTPS)

### Step 2: Check API Response

Use browser DevTools Network tab to see actual API response:

1. Open Developer Tools (F12)
2. Go to Network tab
3. Submit booking form
4. Look for request: `POST /BookingAvailability/CreateWithAvailabilityFlow`
5. Check **Response** tab to see what the API returns

**Expected responses**:
- ✅ `200 OK` with `{ result_type: "BOOKING_CREATED", ... }`
- ✅ `200 OK` with `{ result_type: "ALTERNATIVE_ROOM_SUGGESTED", ... }`
- ✅ `409 Conflict` with `{ result_type: "QUEUED_AND_PENDING_BOOKING_CREATED", ... }`
- ✅ `400 Bad Request` with `{ result_type: "VALIDATION_ERROR", ... }`

**If you see**:
- ❌ `503` error: Database backend not configured
- ❌ `404` error: API endpoint not found (check API is running)
- ❌ `500` error: Server error (check API logs)

### Step 3: Verify Database Migration

The new booking creation requires `booking.room_id` to be nullable.

Check if migration was applied:
```sql
-- Connect to MySQL and run:
USE hotelcarga;
DESCRIBE booking;

-- Look for room_id column:
-- Should show: room_id | int unsigned | NULL | MUL | ...
-- If NULL is YES: Migration applied ✅
-- If NULL is NO: Migration NOT applied ❌
```

**If migration not applied**:
```bash
cd c:\Git\academic-journey\HotelCargaCode
mysql -u<user> -p<password> hotelcarga < sql/migrations/001_allow_nullable_booking_room.sql
```

### Step 4: Check Form Input Values

Open DevTools Console and inspect form before submitting:
```javascript
// Check if form values are valid
document.querySelector('[name="CustomerId"]').value  // Should not be empty or 0
document.querySelector('[name="RoomId"]').value      // Should not be empty or 0
document.querySelector('[name="CheckIn"]').value     // Should be future date
document.querySelector('[name="CheckOut"]').value    // Should be after CheckIn
```

**Common issues**:
- ❌ CustomerId is 0 or missing
- ❌ RoomId is 0 or missing
- ❌ CheckIn is empty or in past
- ❌ CheckOut is before CheckIn

### Step 5: Check Database Connection

Verify API can connect to database:

**In HotelCarga.ApiModel logs, look for**:
```
Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
```

This means DbContext is working.

**If you see**:
```
Cannot connect to server
```

Check connection string in:
- `HotelCarga.ApiModel/appsettings.Development.json`
- User secrets: `dotnet user-secrets list --project HotelCarga.ApiModel`

### Step 6: Verify Database Data

Check that test data exists:
```sql
-- Check customers
SELECT COUNT(*) FROM customer; -- Should be > 0

-- Check rooms
SELECT COUNT(*) FROM room; -- Should be > 0

-- Check room statuses
SELECT * FROM room_status; -- Should have 1=AVAILABLE, 2=OCCUPIED, 3=OUT_OF_ORDER

-- Check specific customer exists
SELECT * FROM customer WHERE id = <CustomerId from form>;

-- Check specific room exists
SELECT * FROM room WHERE id = <RoomId from form>;
```

### Step 7: Check API Logs

Look at API console output or log file for errors:

**Look for**:
```
Error in CreateWithAvailabilityFlow for customer 1
Processing error: ...
```

This indicates what went wrong in the API.

## Common Issues & Solutions

### Issue 1: API Returns 503 "Database backend not configured"

**Cause**: HotelCargaContext is null (dependency injection failed)

**Solution**:
1. Check `HotelCarga.ApiModel/Program.cs` - verify DbContext is registered
2. Verify connection string in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "myConnectionString": "Server=localhost;Database=hotelcarga;User=root;Password=yourpassword;"
     }
   }
   ```
3. Or set via user secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:myConnectionString" "Server=localhost;Database=hotelcarga;User=root;Password=yourpassword;" --project HotelCarga.ApiModel
   ```

### Issue 2: API Returns 404 "Endpoint not found"

**Cause**: API not running or endpoint URL is wrong

**Solution**:
1. Start API:
   ```bash
   cd HotelCarga.ApiModel
   dotnet run
   ```
2. Verify endpoint exists by visiting Swagger: `https://localhost:5001/swagger/ui/index.html`
3. Check Web layer `appsettings.json` for correct API base URL

### Issue 3: API Returns VALIDATION_ERROR "Customer not found"

**Cause**: CustomerId doesn't exist in database

**Solution**:
1. Create a test customer:
   ```sql
   INSERT INTO customer (user_id, first_name, last_name, document_number, phone, address, city, country)
   VALUES (1, 'John', 'Doe', '12345678', '5551234567', '123 Main St', 'CityName', 'CountryName');
   ```
2. Use the generated customer ID in the form

### Issue 4: API Returns VALIDATION_ERROR "Room not found"

**Cause**: RoomId doesn't exist in database

**Solution**:
1. List available rooms:
   ```sql
   SELECT id, room_number FROM room LIMIT 5;
   ```
2. Create a test room if needed:
   ```sql
   INSERT INTO room (room_number, floor_number, category_id, nightly_rate, status_id)
   VALUES (101, 1, 1, 100.00, 1);
   ```
3. Use the correct room ID

### Issue 5: API Returns VALIDATION_ERROR "Check-in date cannot be in the past"

**Cause**: Form is sending past date as check-in

**Solution**:
1. Check form date picker is set to future date
2. Verify browser timezone settings
3. Check server time matches client time

### Issue 6: Booking Created but No Redirect

**Cause**: JavaScript not handling AJAX response

**Solution**:
1. Check browser console for JS errors (F12 → Console tab)
2. Check if Bootstrap or jQuery is loaded
3. Verify response handler JavaScript exists in view

## API Response Examples

### Success: Booking Created (201)
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
    "room_id": 101,
    "customer_id": 1,
    ...
  }
}
```

### Success: Alternative Available (200)
```json
{
  "result_type": "ALTERNATIVE_ROOM_SUGGESTED",
  "success": false,
  "message": "La habitación seleccionada no está disponible, pero encontramos una alternativa.",
  "alternative_room_suggested": {
    "original_requested_room_id": 102,
    "alternative_room_id": 101,
    ...
  }
}
```

### Success: Queued (409)
```json
{
  "result_type": "QUEUED_AND_PENDING_BOOKING_CREATED",
  "success": true,
  "message": "Added to waiting queue.",
  "queued_and_pending": {
    "waiting_queue_id": 1,
    "request_number": "RQ-A1B2C3D4",
    ...
  }
}
```

### Error: Validation (400)
```json
{
  "result_type": "VALIDATION_ERROR",
  "success": false,
  "message": "Check-in date cannot be in the past.",
  "validation_error": {
    "error_code": "INVALID_REQUEST",
    "error_message": "..."
  }
}
```

## Debug Checklist

- [ ] API is running on localhost:5000 or :5001
- [ ] Database migration has been applied (room_id is nullable)
- [ ] Connection string is configured in appsettings or user-secrets
- [ ] Test customer exists in database
- [ ] Test room exists in database
- [ ] Room is marked as AVAILABLE (status_id = 1)
- [ ] Form fields are populated (CustomerId, RoomId, CheckIn, CheckOut)
- [ ] CheckIn date is today or later
- [ ] CheckOut date is after CheckIn date
- [ ] Browser DevTools Network tab shows API response
- [ ] API response includes result_type field
- [ ] No JavaScript errors in browser console

## Need More Help?

Check logs:
1. **API logs**: Run `dotnet run` in HotelCarga.ApiModel and watch console
2. **Web logs**: Run `dotnet run` in HotelCarga.Web and watch console
3. **Browser console**: F12 → Console tab for JS errors
4. **Browser Network**: F12 → Network tab for HTTP details

---

**Last Updated**: 2026-04-19  
**Status**: Troubleshooting Guide v1.0
