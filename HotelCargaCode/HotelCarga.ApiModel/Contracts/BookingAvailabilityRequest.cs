namespace HotelCarga.ApiModel.Contracts;

/// <summary>
/// Request contract for booking with availability checking and waiting queue fallback.
/// Combines booking intent with business logic branching for room availability scenarios.
/// </summary>
public record BookingAvailabilityRequest(
    uint customer_id,
    uint selected_room_id,
    DateTime check_in,
    DateTime check_out,
    bool useJson = false)
{
    /// <summary>
    /// Validates the request for basic preconditions.
    /// </summary>
    public (bool valid, string? error) Validate()
    {
        if (check_in >= check_out)
            return (false, "Check-out date must be after check-in date.");

        if (check_in.Date < DateTime.Today)
            return (false, "Check-in date cannot be in the past.");

        return (true, null);
    }
}
