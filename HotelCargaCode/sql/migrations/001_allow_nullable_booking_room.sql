-- ============================================================================
-- MIGRATION: Allow nullable booking.room_id for PENDING bookings
-- ============================================================================
-- Purpose: Support PENDING bookings where room assignment is deferred until
--          the booking transitions to CONFIRMED status or an alternative room
--          is selected by the customer.
-- Created: 2026-04-19
-- ============================================================================

-- Step 1: Remove foreign key constraint
ALTER TABLE booking 
DROP CONSTRAINT fk_booking_room;

-- Step 2: Drop and recreate indexes if necessary (preserve idx_room_id)
-- Note: The index on room_id can remain; it will continue to work with NULL values

-- Step 3: Modify column to allow NULL
ALTER TABLE booking 
MODIFY COLUMN room_id INT UNSIGNED NULL;

-- Step 4: Recreate foreign key with SetNull behavior
ALTER TABLE booking 
ADD CONSTRAINT fk_booking_room 
FOREIGN KEY (room_id) 
REFERENCES room(id) 
ON DELETE SET NULL 
ON UPDATE CASCADE;

-- Verify the migration
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    DATA_TYPE, 
    COLUMN_KEY 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'booking' 
AND COLUMN_NAME = 'room_id';

-- ============================================================================
-- END MIGRATION
-- ============================================================================
