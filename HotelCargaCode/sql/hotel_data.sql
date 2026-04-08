-- ============================================================================
-- HOTEL MANAGEMENT SYSTEM - Data Seed Script
-- ============================================================================
-- Contains DELETE statements followed by INSERTs for reference data.
-- Running this script will clear all existing data and then populate
-- the lookup tables. This script can be executed repeatedly.
-- ============================================================================

-- Disable foreign key checks for safe data purging
SET FOREIGN_KEY_CHECKS = 0;

-- Delete all data in reverse order of dependencies
DELETE FROM waiting_queue;
DELETE FROM booking_history;
DELETE FROM booking;
DELETE FROM room_availability;
DELETE FROM room;
DELETE FROM customer;
DELETE FROM user;
DELETE FROM booking_status;
DELETE FROM queue_status;
DELETE FROM room_status;
DELETE FROM room_category;
DELETE FROM role;
DELETE FROM user_status;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- Insert Reference/Lookup Data
-- ============================================================================

-- Insert User Status records
INSERT INTO user_status (status_name, description) VALUES 
    ('ACTIVE', 'User account is active'),
    ('DISABLED', 'User account is disabled');

-- Insert Role records
INSERT INTO role (role_name, description) VALUES 
    ('CUSTOMER', 'Customer role for hotel guests'),
    ('ADMIN', 'Administrator role with full access'),
    ('OPERATIVE', 'Operative role for hotel staff');

-- Insert Room Status records
INSERT INTO room_status (status_name, description) VALUES 
    ('AVAILABLE', 'Room is available for booking'),
    ('OCCUPIED', 'Room is occupied by a guest'),
    ('OUT OF ORDER', 'Room is not available for maintenance or repairs');

-- Insert Room Category records
INSERT INTO room_category (category_name, description) VALUES 
    ('SENCILLA', 'Simple room with basic amenities'),
    ('DELUXE', 'Deluxe room with premium amenities'),
    ('ULTRA PREMIUM', 'Ultra premium room with luxury amenities and services');

-- Insert Booking Status records
INSERT INTO booking_status (status_name, description) VALUES 
    ('PENDING', 'Booking is pending confirmation'),
    ('CONFIRMED', 'Booking is confirmed'),
    ('CANCELED', 'Booking is cancelled');

-- Insert Queue Status records
INSERT INTO queue_status (status_name, description) VALUES 
    ('PENDING', 'Queue entry is pending'),
    ('CONFIRMED', 'Queue entry is confirmed'),
    ('CANCELED', 'Queue entry is cancelled'),
    ('DISCARDED', 'Queue entry is discarded (outdated)');

-- ============================================================================
-- End of Data Seed Script
-- ============================================================================
