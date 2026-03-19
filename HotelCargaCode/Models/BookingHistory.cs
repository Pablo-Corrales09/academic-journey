using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Audit trail tracking all booking changes and historical events
/// </summary>
public partial class BookingHistory
{
    public uint Id { get; set; }

    public uint BookingId { get; set; }

    public uint CustomerId { get; set; }

    public uint RoomId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public byte StatusId { get; set; }

    public string ActionType { get; set; }

    public decimal? NightlyRate { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking Booking { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Room Room { get; set; }

    public virtual BookingStatus Status { get; set; }
}
