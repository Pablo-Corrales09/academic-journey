using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

public partial class VwCustomerBooking
{
    public uint CustomerId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string DocumentNumber { get; set; }

    public string Phone { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Country { get; set; }

    public uint? BookingId { get; set; }

    public string ReserveNumber { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public decimal? NightlyRate { get; set; }

    public decimal? TotalPrice { get; set; }

    public string BookingStatus { get; set; }

    public uint? RoomNumber { get; set; }

    public byte? FloorNumber { get; set; }

    public string RoomCategory { get; set; }

    public string RoomStatus { get; set; }

    public DateTime? BookingCreatedAt { get; set; }

    public DateTime? BookingUpdatedAt { get; set; }
}
