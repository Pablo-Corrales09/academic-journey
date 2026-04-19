-- ============================================================================
-- MIGRATION: Enable pending bookings with deferred room assignment
-- Purpose:
--   1) Allow booking.room_id to be NULL for PENDING bookings.
--   2) Allow booking_history.room_id to be NULL so INSERT trigger does not fail.
-- ============================================================================

-- booking.room_id
ALTER TABLE booking DROP FOREIGN KEY fk_booking_room;
ALTER TABLE booking MODIFY room_id INT UNSIGNED NULL;
ALTER TABLE booking
    ADD CONSTRAINT fk_booking_room
    FOREIGN KEY (room_id)
    REFERENCES room(id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

-- booking_history.room_id
ALTER TABLE booking_history DROP FOREIGN KEY fk_booking_history_room;
ALTER TABLE booking_history MODIFY room_id INT UNSIGNED NULL;
ALTER TABLE booking_history
    ADD CONSTRAINT fk_booking_history_room
    FOREIGN KEY (room_id)
    REFERENCES room(id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;
