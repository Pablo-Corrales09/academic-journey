using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Booking reservations with customer, room, and pricing details
/// </summary>
public partial class Booking
{
    public uint Id { get; set; }

    public string ReserveNumber { get; set; }

    public uint CustomerId { get; set; }

    public uint RoomId { get; set; }

    public byte StatusId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal NightlyRate { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual Customer Customer { get; set; }

    public virtual Room Room { get; set; }

    public virtual BookingStatus Status { get; set; }
}
