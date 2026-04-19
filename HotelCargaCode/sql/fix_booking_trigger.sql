-- ============================================================================
-- MIGRATION: Fix booking date validation trigger
-- Apply this script to any running database instance.
-- Fixes: trg_check_booking_dates_not_past was using NOW() (datetime comparison)
--        which rejected any booking inserted after midnight on check-in date.
--        Changed to CURDATE() (date-only comparison) to allow same-day bookings.
-- ============================================================================

DROP TRIGGER IF EXISTS trg_check_booking_dates_not_past;

DELIMITER $$

CREATE TRIGGER trg_check_booking_dates_not_past
BEFORE INSERT ON booking
FOR EACH ROW
BEGIN
    IF DATE(NEW.check_in) < CURDATE() OR DATE(NEW.check_out) < CURDATE() THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Las fechas de la reserva no pueden ser anteriores a hoy';
    END IF;
END$$

DELIMITER ;
