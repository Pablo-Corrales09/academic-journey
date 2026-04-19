namespace HotelCarga.ApiModel.Contracts;

/// <summary>
/// Unified response for booking availability orchestration.
/// Result type indicates which business flow was executed.
/// </summary>
public class BookingAvailabilityResponse
{
    /// <summary>
    /// Result code for the availability check: BOOKING_CREATED, ALTERNATIVE_ROOM_SUGGESTED, QUEUED_AND_PENDING_BOOKING_CREATED, or VALIDATION_ERROR
    /// </summary>
    public string result_type { get; set; } = string.Empty;

    /// <summary>
    /// High-level success/error indicator.
    /// </summary>
    public bool success { get; set; }

    /// <summary>
    /// Human-readable message describing the outcome.
    /// </summary>
    public string message { get; set; } = string.Empty;

    /// <summary>
    /// Populated when result_type = BOOKING_CREATED.
    /// </summary>
    public BookingDataPayload? booking_created { get; set; }

    /// <summary>
    /// Populated when result_type = ALTERNATIVE_ROOM_SUGGESTED.
    /// Contains the first available room in the same category as the originally requested room.
    /// </summary>
    public AlternativeRoomPayload? alternative_room_suggested { get; set; }

    /// <summary>
    /// Populated when result_type = QUEUED_AND_PENDING_BOOKING_CREATED.
    /// Contains waiting queue and pending booking details when all rooms in category are occupied.
    /// </summary>
    public QueuedAndPendingPayload? queued_and_pending { get; set; }

    /// <summary>
    /// Populated when result_type = VALIDATION_ERROR.
    /// Contains validation error details.
    /// </summary>
    public ValidationErrorPayload? validation_error { get; set; }
}

/// <summary>
/// Booking data returned when booking is successfully created.
/// </summary>
public class BookingDataPayload
{
    public uint id { get; set; }
    public string? reserve_number { get; set; }
    public byte status_id { get; set; }
    public string status_name { get; set; } = string.Empty;
    public uint customer_id { get; set; }
    public uint room_id { get; set; }
    public uint room_number { get; set; }
    public DateTime check_in { get; set; }
    public DateTime check_out { get; set; }
    public decimal nightly_rate { get; set; }
    public decimal total_price { get; set; }
    public DateTime? created_at { get; set; }
}

/// <summary>
/// Alternative room information returned when selected room is occupied but alternatives exist.
/// Client should prompt user to accept or retry with different criteria.
/// </summary>
public class AlternativeRoomPayload
{
    public uint original_requested_room_id { get; set; }
    public uint original_requested_room_number { get; set; }
    public string room_category_name { get; set; } = string.Empty;
    public uint alternative_room_id { get; set; }
    public uint alternative_room_number { get; set; }
    public byte alternative_room_status_id { get; set; }
    public string alternative_room_status_name { get; set; } = string.Empty;
    public decimal alternative_nightly_rate { get; set; }
    public byte floor_number { get; set; }
}

/// <summary>
/// Waiting queue and pending booking details when all rooms in category are occupied.
/// </summary>
public class QueuedAndPendingPayload
{
    /// <summary>
    /// Waiting queue record ID.
    /// </summary>
    public uint waiting_queue_id { get; set; }

    /// <summary>
    /// Formatted request number (RQ-XXXXXXXX).
    /// </summary>
    public string request_number { get; set; } = string.Empty;

    /// <summary>
    /// Pending booking record ID (room_id will be NULL).
    /// </summary>
    public uint pending_booking_id { get; set; }

    /// <summary>
    /// Reserve number for the pending booking.
    /// </summary>
    public string? pending_reserve_number { get; set; }

    /// <summary>
    /// Room category name requested.
    /// </summary>
    public string room_category_name { get; set; } = string.Empty;

    /// <summary>
    /// Requested check-in date.
    /// </summary>
    public DateTime requested_check_in { get; set; }

    /// <summary>
    /// Requested check-out date.
    /// </summary>
    public DateTime requested_check_out { get; set; }

    /// <summary>
    /// Queue status (always "PENDING" initially).
    /// </summary>
    public string queue_status_name { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when queue entry was created.
    /// </summary>
    public DateTime? created_at { get; set; }
}

/// <summary>
/// Validation error details.
/// </summary>
public class ValidationErrorPayload
{
    public string error_code { get; set; } = string.Empty;
    public string error_message { get; set; } = string.Empty;
    public Dictionary<string, string>? field_errors { get; set; }
}
