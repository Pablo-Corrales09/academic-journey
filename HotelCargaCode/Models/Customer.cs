using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Customer profiles with personal identification and contact information
/// </summary>
public partial class Customer
{
    public uint Id { get; set; }

    public uint UserId { get; set; }

    public string DocumentNumber { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual User User { get; set; }

    public virtual ICollection<WaitingQueue> WaitingQueues { get; set; } = new List<WaitingQueue>();
}
