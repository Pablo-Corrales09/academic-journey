using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Physical rooms with pricing, category, and status information
/// </summary>
public partial class Room
{
    public uint Id { get; set; }

    public uint RoomNumber { get; set; }

    public byte StatusId { get; set; }

    public byte CategoryId { get; set; }

    public decimal NightlyRate { get; set; }

    public byte? FloorNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual RoomCategory Category { get; set; }

    public virtual ICollection<RoomAvailability> RoomAvailabilities { get; set; } = new List<RoomAvailability>();

    public virtual RoomStatus Status { get; set; }
}
