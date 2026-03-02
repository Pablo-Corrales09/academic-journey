-- ============================================================================
-- HOTEL MANAGEMENT SYSTEM - MySQL 8.4 DDL Schema
-- ============================================================================
-- Last Updated: Feb 28, 2026
-- Purpose: Complete database schema for hotel_carga
-- Naming Convention: snake_case for all tables and columns
-- Developer: Pablo Corrales Padilla
-- ============================================================================

-- Drop existing objects to allow redeployment
-- ============================================================================

DROP EVENT IF EXISTS evt_update_room_status_cleanup;
DROP PROCEDURE IF EXISTS sp_queue_cleanup;
DROP PROCEDURE IF EXISTS sp_cancel_booking;
DROP PROCEDURE IF EXISTS sp_reschedule_booking;
DROP PROCEDURE IF EXISTS sp_register_booking;
DROP PROCEDURE IF EXISTS sp_generate_weekly_availability;
DROP TRIGGER IF EXISTS trg_booking_checkout_mark_available;
DROP TRIGGER IF EXISTS trg_booking_canceled_room_available;
DROP TRIGGER IF EXISTS trg_booking_confirmed_room_occupied;
DROP TRIGGER IF EXISTS trg_prevent_double_booking;
DROP TRIGGER IF EXISTS trg_check_room_available_before_confirm;
DROP TRIGGER IF EXISTS trg_check_booking_dates_not_past;
DROP TRIGGER IF EXISTS trg_booking_insert_history;
DROP TRIGGER IF EXISTS trg_user_default_status;
DROP VIEW IF EXISTS vw_waiting_queue_report;
DROP VIEW IF EXISTS vw_customer_bookings;

-- Drop tables in reverse order of dependencies
DROP TABLE IF EXISTS waiting_queue;
DROP TABLE IF EXISTS booking_history;
DROP TABLE IF EXISTS booking;
DROP TABLE IF EXISTS room_availability;
DROP TABLE IF EXISTS room;
DROP TABLE IF EXISTS customer;
DROP TABLE IF EXISTS booking_status;
DROP TABLE IF EXISTS queue_status;
DROP TABLE IF EXISTS room_status;
DROP TABLE IF EXISTS room_category;
DROP TABLE IF EXISTS role;
DROP TABLE IF EXISTS user_status;
DROP TABLE IF EXISTS user;

-- ============================================================================
-- 1. USER AND ROLE REFERENCE TABLES
-- ============================================================================

-- user_status: Stores possible states for user accounts (ACTIVE, DISABLED)
CREATE TABLE user_status (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    status_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_user_status_name CHECK (
        status_name IN ('ACTIVE', 'DISABLED')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for user account statuses';

-- role: Stores system roles (CUSTOMER, ADMIN, OPERATIVE)
CREATE TABLE role (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    role_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_role_name CHECK (
        role_name IN ('CUSTOMER', 'ADMIN', 'OPERATIVE')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for user roles';

-- user: Main user account table with authentication and role assignment
CREATE TABLE user (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    status_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_username_not_empty CHECK (LENGTH(TRIM(username)) > 0),
    CONSTRAINT fk_user_role FOREIGN KEY (role_id) 
        REFERENCES role(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_user_status FOREIGN KEY (status_id) 
        REFERENCES user_status(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_username (username),
    INDEX idx_email (email),
    INDEX idx_status_id (status_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='User accounts with authentication and role assignment';

-- ============================================================================
-- 2. CUSTOMER TABLE
-- ============================================================================

-- customer: Stores customer profile information linked to user accounts
CREATE TABLE customer (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    user_id INT UNSIGNED NOT NULL UNIQUE,
    document_number VARCHAR(50) NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    address VARCHAR(255),
    city VARCHAR(100),
    country VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_document_number_alphanumeric CHECK (
        document_number REGEXP '^[a-zA-Z0-9]+$'
    ),
    CONSTRAINT chk_document_number_not_empty CHECK (LENGTH(TRIM(document_number)) > 0),
    CONSTRAINT fk_customer_user FOREIGN KEY (user_id) 
        REFERENCES user(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_document_number (document_number),
    INDEX idx_user_id (user_id),
    INDEX idx_last_name (last_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Customer profiles with personal identification and contact information';

-- ============================================================================
-- 3. ROOM REFERENCE TABLES
-- ============================================================================

-- room_status: Reference table for room availability states (AVAILABLE, OCCUPIED, OUT OF ORDER)
CREATE TABLE room_status (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    status_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_room_status_name CHECK (
        status_name IN ('AVAILABLE', 'OCCUPIED', 'OUT OF ORDER')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for room availability states';

-- room_category: Reference table for room types (SENCILLA, DELUXE, ULTRA PREMIUM)
CREATE TABLE room_category (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    category_name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    amenities VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_room_category_name CHECK (
        category_name IN ('SENCILLA', 'DELUXE', 'ULTRA PREMIUM')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for room categories and types';

-- room: Physical rooms with pricing and status tracking
CREATE TABLE room (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    room_number INT UNSIGNED NOT NULL UNIQUE,
    status_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    category_id TINYINT UNSIGNED NOT NULL,
    nightly_rate DECIMAL(10, 2) NOT NULL,
    floor_number TINYINT UNSIGNED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_room_number_positive CHECK (room_number > 0),
    CONSTRAINT chk_nightly_rate_non_negative CHECK (nightly_rate >= 0),
    CONSTRAINT fk_room_status FOREIGN KEY (status_id) 
        REFERENCES room_status(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_room_category FOREIGN KEY (category_id) 
        REFERENCES room_category(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_room_number (room_number),
    INDEX idx_status_id (status_id),
    INDEX idx_category_id (category_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Physical rooms with pricing, category, and status information';

-- room_availability: Tracks availability periods for rooms across dates
CREATE TABLE room_availability (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    room_id INT UNSIGNED NOT NULL,
    start_schedule DATETIME NOT NULL,
    end_schedule DATETIME NOT NULL,
    creation_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_schedule_order CHECK (end_schedule > start_schedule),
    CONSTRAINT fk_room_availability_room FOREIGN KEY (room_id) 
        REFERENCES room(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    UNIQUE INDEX uk_room_availability (room_id, start_schedule, end_schedule),
    INDEX idx_start_schedule (start_schedule),
    INDEX idx_end_schedule (end_schedule)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Room availability schedule with date range management';

-- ============================================================================
-- 4. BOOKING REFERENCE TABLES
-- ============================================================================

-- booking_status: Reference table for booking statuses (PENDING, CONFIRMED, CANCELED)
CREATE TABLE booking_status (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    status_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_booking_status_name CHECK (
        status_name IN ('PENDING', 'CONFIRMED', 'CANCELED')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for booking statuses';

-- booking: Main booking table with reservation details and pricing
CREATE TABLE booking (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    reserve_number VARCHAR(50) NOT NULL UNIQUE,
    customer_id INT UNSIGNED NOT NULL,
    room_id INT UNSIGNED NOT NULL,
    status_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    check_in DATETIME NOT NULL,
    check_out DATETIME NOT NULL,
    nightly_rate DECIMAL(10, 2) NOT NULL,
    total_price DECIMAL(12, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_checkout_greater_checkin CHECK (check_out > check_in),
    CONSTRAINT chk_reserve_number_alphanumeric CHECK (
        reserve_number REGEXP '^[a-zA-Z0-9]+$'
    ),
    CONSTRAINT chk_nightly_rate_booking_non_negative CHECK (nightly_rate >= 0),
    CONSTRAINT chk_total_price_non_negative CHECK (total_price >= 0),
    CONSTRAINT fk_booking_customer FOREIGN KEY (customer_id) 
        REFERENCES customer(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_booking_room FOREIGN KEY (room_id) 
        REFERENCES room(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_booking_status FOREIGN KEY (status_id) 
        REFERENCES booking_status(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_reserve_number (reserve_number),
    INDEX idx_customer_id (customer_id),
    INDEX idx_room_id (room_id),
    INDEX idx_status_id (status_id),
    INDEX idx_check_in (check_in),
    INDEX idx_check_out (check_out)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Booking reservations with customer, room, and pricing details';

-- booking_history: Historical audit trail of booking changes and statuses
CREATE TABLE booking_history (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    booking_id INT UNSIGNED NOT NULL,
    customer_id INT UNSIGNED NOT NULL,
    room_id INT UNSIGNED NOT NULL,
    check_in DATETIME NOT NULL,
    check_out DATETIME NOT NULL,
    status_id TINYINT UNSIGNED NOT NULL,
    action_type VARCHAR(50) NOT NULL,
    nightly_rate DECIMAL(10, 2),
    total_price DECIMAL(12, 2),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_booking_history_booking FOREIGN KEY (booking_id) 
        REFERENCES booking(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_booking_history_customer FOREIGN KEY (customer_id) 
        REFERENCES customer(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_booking_history_room FOREIGN KEY (room_id) 
        REFERENCES room(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_booking_history_status FOREIGN KEY (status_id) 
        REFERENCES booking_status(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_booking_id (booking_id),
    INDEX idx_customer_id (customer_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Audit trail tracking all booking changes and historical events';

-- ============================================================================
-- 5. WAITING QUEUE TABLES
-- ============================================================================

-- queue_status: Reference table for queue entry statuses (PENDING, CONFIRMED, CANCELED, DISCARDED)
CREATE TABLE queue_status (
    id TINYINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    status_name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_queue_status_name CHECK (
        status_name IN ('PENDING', 'CONFIRMED', 'CANCELED', 'DISCARDED')
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Reference table for waiting queue entry statuses';

-- waiting_queue: Queue system for customers waiting for room availability
CREATE TABLE waiting_queue (
    id INT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    customer_id INT UNSIGNED NOT NULL,
    room_category_id TINYINT UNSIGNED NOT NULL,
    status_id TINYINT UNSIGNED NOT NULL DEFAULT 1,
    requested_check_in DATETIME NOT NULL,
    check_out DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT chk_requested_checkin_not_past CHECK (requested_check_in >= CURDATE()),
    CONSTRAINT fk_waiting_queue_customer FOREIGN KEY (customer_id) 
        REFERENCES customer(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_waiting_queue_category FOREIGN KEY (room_category_id) 
        REFERENCES room_category(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_waiting_queue_status FOREIGN KEY (status_id) 
        REFERENCES queue_status(id) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_customer_id (customer_id),
    INDEX idx_status_id (status_id),
    INDEX idx_requested_check_in (requested_check_in)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
COMMENT='Waiting queue for customers seeking room availability';

-- ============================================================================
-- 6. TRIGGERS
-- ============================================================================

DELIMITER $$

-- Trigger: Set user default status to ACTIVE when created
CREATE TRIGGER trg_user_default_status
BEFORE INSERT ON user
FOR EACH ROW
BEGIN
    IF NEW.status_id IS NULL THEN
        SET NEW.status_id = (SELECT id FROM user_status WHERE status_name = 'ACTIVE');
    END IF;
END$$

-- Trigger: Validate booking dates are not lower than actual DATETIME
CREATE TRIGGER trg_check_booking_dates_not_past
BEFORE INSERT ON booking
FOR EACH ROW
BEGIN
    IF NEW.check_in < NOW() OR NEW.check_out < NOW() THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Booking dates cannot be lower than current DATETIME';
    END IF;
END$$

-- Trigger: Prevent double booking - room cannot be booked twice at same DATETIME
CREATE TRIGGER trg_prevent_double_booking
BEFORE INSERT ON booking
FOR EACH ROW
BEGIN
    IF EXISTS (
        SELECT 1 FROM booking 
        WHERE room_id = NEW.room_id 
        AND status_id IN (SELECT id FROM booking_status WHERE status_name IN ('PENDING', 'CONFIRMED'))
        AND (
            (NEW.check_in < check_out AND NEW.check_out > check_in)
        )
    ) THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Room is already booked for this period';
    END IF;
END$$

-- Trigger: Booking can only be confirmed if room is available
CREATE TRIGGER trg_check_room_available_before_confirm
BEFORE UPDATE ON booking
FOR EACH ROW
BEGIN
    IF NEW.status_id = (SELECT id FROM booking_status WHERE status_name = 'CONFIRMED') 
       AND OLD.status_id <> NEW.status_id THEN
        IF (SELECT status_id FROM room WHERE id = NEW.room_id) 
           NOT IN (SELECT id FROM room_status WHERE status_name = 'AVAILABLE') THEN
            SIGNAL SQLSTATE '45000' 
            SET MESSAGE_TEXT = 'Room must be AVAILABLE to confirm booking. Room is OCCUPIED or OUT OF ORDER';
        END IF;
    END IF;
END$$

-- Trigger: When booking is CONFIRMED, update room status to OCCUPIED
CREATE TRIGGER trg_booking_confirmed_room_occupied
AFTER UPDATE ON booking
FOR EACH ROW
BEGIN
    IF NEW.status_id = (SELECT id FROM booking_status WHERE status_name = 'CONFIRMED') 
       AND OLD.status_id <> NEW.status_id THEN
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'OCCUPIED')
        WHERE id = NEW.room_id;
    END IF;
END$$

-- Trigger: When booking is CANCELED, update room status to AVAILABLE
CREATE TRIGGER trg_booking_canceled_room_available
AFTER UPDATE ON booking
FOR EACH ROW
BEGIN
    IF NEW.status_id = (SELECT id FROM booking_status WHERE status_name = 'CANCELED') THEN
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'AVAILABLE')
        WHERE id = NEW.room_id;
    END IF;
END$$

-- Trigger: Insert into booking_history when booking is registered
CREATE TRIGGER trg_booking_insert_history
AFTER INSERT ON booking
FOR EACH ROW
BEGIN
    INSERT INTO booking_history (
        booking_id, customer_id, room_id, check_in, check_out, 
        status_id, action_type, nightly_rate, total_price, created_at
    )
    VALUES (
        NEW.id, NEW.customer_id, NEW.room_id, NEW.check_in, NEW.check_out, 
        NEW.status_id, 'CREATE', NEW.nightly_rate, NEW.total_price, NOW()
    );
END$$

-- Trigger: Mark room as AVAILABLE when checkout time is reached
CREATE TRIGGER trg_booking_checkout_mark_available
AFTER UPDATE ON booking
FOR EACH ROW
BEGIN
    IF NEW.status_id = (SELECT id FROM booking_status WHERE status_name = 'CONFIRMED')
       AND NEW.check_out <= NOW() THEN
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'AVAILABLE')
        WHERE id = NEW.room_id;
    END IF;
END$$

DELIMITER ;

-- ============================================================================
-- 7. STORED PROCEDURES
-- ============================================================================

DELIMITER $$

-- Procedure: Generate weekly availability for all rooms
CREATE PROCEDURE sp_generate_weekly_availability(
    IN p_start_date DATE,
    IN p_end_date DATE
)
MODIFIES SQL DATA
SQL SECURITY DEFINER
BEGIN
    DECLARE v_day_count INT;
    DECLARE v_current_date DATE;
    DECLARE v_room_id INT;
    DECLARE v_done INT DEFAULT FALSE;
    DECLARE v_room_cursor CURSOR FOR SELECT id FROM room;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = TRUE;
    
    SET v_day_count = DATEDIFF(p_end_date, p_start_date);
    
    IF v_day_count > 15 OR v_day_count < 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Date range cannot exceed 15 days';
    END IF;
    
    START TRANSACTION;
    OPEN v_room_cursor;
    
    read_loop: LOOP
        FETCH v_room_cursor INTO v_room_id;
        IF v_done THEN
            LEAVE read_loop;
        END IF;
        
        SET v_current_date = p_start_date;
        WHILE v_current_date < p_end_date DO
            INSERT IGNORE INTO room_availability (room_id, start_schedule, end_schedule, creation_at)
            VALUES (
                v_room_id,
                CONCAT(v_current_date, ' 14:00:00'),
                CONCAT(DATE_ADD(v_current_date, INTERVAL 1 DAY), ' 11:00:00'),
                NOW()
            );
            SET v_current_date = DATE_ADD(v_current_date, INTERVAL 1 DAY);
        END WHILE;
    END LOOP;
    
    CLOSE v_room_cursor;
    COMMIT;
END$$

-- Procedure: Register a new booking
CREATE PROCEDURE sp_register_booking(
    IN p_customer_name VARCHAR(100),
    IN p_room_id INT UNSIGNED,
    IN p_check_in DATETIME,
    IN p_check_out DATETIME,
    IN p_nightly_rate DECIMAL(10, 2),
    OUT p_booking_id INT UNSIGNED,
    OUT p_message VARCHAR(255)
)
MODIFIES SQL DATA
SQL SECURITY DEFINER
BEGIN
    DECLARE v_customer_id INT UNSIGNED;
    DECLARE v_room_status INT;
    DECLARE v_reserve_number VARCHAR(50);
    DECLARE v_total_price DECIMAL(12, 2);
    DECLARE v_booking_status_id INT;
    
    START TRANSACTION;
    
    BEGIN
        DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            ROLLBACK;
            SET p_message = 'Error: Booking registration failed';
            SET p_booking_id = NULL;
        END;
        
        SELECT id INTO v_customer_id FROM customer 
        WHERE CONCAT(first_name, ' ', last_name) LIKE CONCAT('%', p_customer_name, '%')
        LIMIT 1;
        
        IF v_customer_id IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Customer not found';
        END IF;
        
        SELECT status_id INTO v_room_status FROM room WHERE id = p_room_id;
        
        IF v_room_status NOT IN (SELECT id FROM room_status WHERE status_name = 'AVAILABLE') THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Room is not available for booking';
        END IF;
        
        IF EXISTS (
            SELECT 1 FROM booking 
            WHERE room_id = p_room_id 
            AND status_id IN (SELECT id FROM booking_status WHERE status_name IN ('PENDING', 'CONFIRMED'))
            AND (
                (p_check_in < check_out AND p_check_out > check_in)
            )
        ) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Room is already booked for this period';
        END IF;
        
        SET v_reserve_number = CONCAT('RES', DATE_FORMAT(NOW(), '%Y%m%d'), FLOOR(RAND() * 10000));
        SET v_total_price = p_nightly_rate * DATEDIFF(p_check_out, p_check_in);
        SET v_booking_status_id = (SELECT id FROM booking_status WHERE status_name = 'PENDING');
        
        INSERT INTO booking (reserve_number, customer_id, room_id, status_id, check_in, check_out, nightly_rate, total_price)
        VALUES (v_reserve_number, v_customer_id, p_room_id, v_booking_status_id, p_check_in, p_check_out, p_nightly_rate, v_total_price);
        
        SET p_booking_id = LAST_INSERT_ID();
        SET p_message = CONCAT('Booking created successfully with reserve number: ', v_reserve_number);
        
        COMMIT;
    END;
END$$

-- Procedure: Reschedule a booking
CREATE PROCEDURE sp_reschedule_booking(
    IN p_customer_name VARCHAR(100),
    IN p_new_check_in DATETIME,
    IN p_new_check_out DATETIME,
    IN p_new_room_id INT UNSIGNED,
    OUT p_message VARCHAR(255)
)
MODIFIES SQL DATA
SQL SECURITY DEFINER
BEGIN
    DECLARE v_customer_id INT UNSIGNED;
    DECLARE v_booking_id INT UNSIGNED;
    DECLARE v_old_room_id INT UNSIGNED;
    
    START TRANSACTION;
    
    BEGIN
        DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            ROLLBACK;
            SET p_message = 'Error: Rescheduling failed';
        END;
        
        SELECT id INTO v_customer_id FROM customer 
        WHERE CONCAT(first_name, ' ', last_name) LIKE CONCAT('%', p_customer_name, '%')
        LIMIT 1;
        
        IF v_customer_id IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Customer not found';
        END IF;
        
        SELECT id, room_id INTO v_booking_id, v_old_room_id FROM booking 
        WHERE customer_id = v_customer_id 
        AND status_id IN (SELECT id FROM booking_status WHERE status_name IN ('PENDING', 'CONFIRMED'))
        LIMIT 1;
        
        IF v_booking_id IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No active booking found for customer';
        END IF;
        
        IF p_new_check_in < NOW() OR p_new_check_out < NOW() THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'New dates cannot be in the past';
        END IF;
        
        UPDATE booking 
        SET room_id = p_new_room_id, check_in = p_new_check_in, check_out = p_new_check_out
        WHERE id = v_booking_id;
        
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'AVAILABLE')
        WHERE id = v_old_room_id;
        
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'OCCUPIED')
        WHERE id = p_new_room_id;
        
        INSERT INTO booking_history (booking_id, customer_id, room_id, check_in, check_out, status_id, action_type, created_at)
        SELECT id, customer_id, room_id, check_in, check_out, status_id, 'RESCHEDULE', NOW()
        FROM booking WHERE id = v_booking_id;
        
        SET p_message = 'Booking rescheduled successfully';
        COMMIT;
    END;
END$$

-- Procedure: Cancel a booking
CREATE PROCEDURE sp_cancel_booking(
    IN p_customer_name VARCHAR(100),
    OUT p_message VARCHAR(255)
)
MODIFIES SQL DATA
SQL SECURITY DEFINER
BEGIN
    DECLARE v_customer_id INT UNSIGNED;
    DECLARE v_booking_id INT UNSIGNED;
    DECLARE v_room_id INT UNSIGNED;
    DECLARE v_canceled_status_id INT;
    
    START TRANSACTION;
    
    BEGIN
        DECLARE EXIT HANDLER FOR SQLEXCEPTION
        BEGIN
            ROLLBACK;
            SET p_message = 'Error: Cancellation failed';
        END;
        
        SELECT id INTO v_customer_id FROM customer 
        WHERE CONCAT(first_name, ' ', last_name) LIKE CONCAT('%', p_customer_name, '%')
        LIMIT 1;
        
        IF v_customer_id IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Customer not found';
        END IF;
        
        SELECT id, room_id INTO v_booking_id, v_room_id FROM booking 
        WHERE customer_id = v_customer_id 
        AND status_id IN (SELECT id FROM booking_status WHERE status_name IN ('PENDING', 'CONFIRMED'))
        LIMIT 1;
        
        IF v_booking_id IS NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No active booking found for customer';
        END IF;
        
        SET v_canceled_status_id = (SELECT id FROM booking_status WHERE status_name = 'CANCELED');
        
        UPDATE booking 
        SET status_id = v_canceled_status_id
        WHERE id = v_booking_id;
        
        UPDATE room 
        SET status_id = (SELECT id FROM room_status WHERE status_name = 'AVAILABLE')
        WHERE id = v_room_id;
        
        INSERT INTO booking_history (booking_id, customer_id, room_id, check_in, check_out, status_id, action_type, created_at)
        SELECT id, customer_id, room_id, check_in, check_out, status_id, 'CANCEL', NOW()
        FROM booking WHERE id = v_booking_id;
        
        SET p_message = 'Booking cancelled successfully';
        COMMIT;
    END;
END$$

-- Procedure: Clean up waiting queue
CREATE PROCEDURE sp_queue_cleanup()
MODIFIES SQL DATA
SQL SECURITY DEFINER
BEGIN
    DECLARE v_discarded_status_id INT;
    
    SELECT id INTO v_discarded_status_id FROM queue_status WHERE status_name = 'DISCARDED';
    
    UPDATE waiting_queue 
    SET status_id = v_discarded_status_id
    WHERE status_id NOT IN (SELECT id FROM queue_status WHERE status_name = 'DISCARDED')
    AND requested_check_in < CURDATE();
END$$

DELIMITER ;

-- ============================================================================
-- 8. VIEWS
-- ============================================================================

-- View: Customer Bookings with full details
CREATE VIEW vw_customer_bookings AS
SELECT 
    c.id AS customer_id,
    c.first_name,
    c.last_name,
    c.document_number,
    c.phone,
    u.email,
    c.address,
    c.city,
    c.country,
    b.id AS booking_id,
    b.reserve_number,
    b.check_in,
    b.check_out,
    b.nightly_rate,
    b.total_price,
    bs.status_name AS booking_status,
    r.room_number,
    r.floor_number,
    rc.category_name AS room_category,
    rs.status_name AS room_status,
    b.created_at AS booking_created_at,
    b.updated_at AS booking_updated_at
FROM customer c
INNER JOIN user u ON c.user_id = u.id
LEFT JOIN booking b ON c.id = b.customer_id
LEFT JOIN booking_status bs ON b.status_id = bs.id
LEFT JOIN room r ON b.room_id = r.id
LEFT JOIN room_category rc ON r.category_id = rc.id
LEFT JOIN room_status rs ON r.status_id = rs.id
ORDER BY c.id, b.created_at DESC;

-- View: Waiting Queue Report with dynamic status
CREATE VIEW vw_waiting_queue_report AS
SELECT 
    wq.id AS queue_id,
    c.id AS customer_id,
    c.first_name,
    c.last_name,
    c.document_number,
    rc.category_name AS requested_room_category,
    wq.requested_check_in,
    wq.check_out,
    qs.status_name AS queue_status,
    CASE 
        WHEN wq.requested_check_in < CURDATE() THEN 'DISCARDED'
        ELSE 'ACTIVO'
    END AS current_status,
    wq.created_at,
    wq.updated_at
FROM waiting_queue wq
INNER JOIN customer c ON wq.customer_id = c.id
INNER JOIN room_category rc ON wq.room_category_id = rc.id
INNER JOIN queue_status qs ON wq.status_id = qs.id
ORDER BY wq.created_at ASC;

-- ============================================================================
-- 9. MYSQL EVENT
-- ============================================================================

DELIMITER $$

-- Event: Update room status and clean waiting queue every 15 minutes
CREATE EVENT evt_update_room_status_cleanup
ON SCHEDULE EVERY 15 MINUTE
STARTS CURRENT_TIMESTAMP
ENABLE
DO
BEGIN
    DECLARE v_available_status_id INT;
    DECLARE v_confirmed_status_id INT;
    
    SELECT id INTO v_available_status_id FROM room_status WHERE status_name = 'AVAILABLE';
    SELECT id INTO v_confirmed_status_id FROM booking_status WHERE status_name = 'CONFIRMED';
    
    UPDATE room r
    SET status_id = v_available_status_id
    WHERE id IN (
        SELECT DISTINCT room_id 
        FROM booking 
        WHERE status_id = v_confirmed_status_id 
        AND check_out <= NOW()
    );
    
    CALL sp_queue_cleanup();
END$$

DELIMITER ;

-- ============================================================================
-- END OF SCHEMA DEFINITION
-- ============================================================================
