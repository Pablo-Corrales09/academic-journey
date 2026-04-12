using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Booking reservations with customer, room, and pricing details
/// </summary>
public partial class booking
{
    public uint id { get; set; }

    public string reserve_number { get; set; }

    public uint customer_id { get; set; }

    public uint room_id { get; set; }

    public byte status_id { get; set; }

    public DateTime check_in { get; set; }

    public DateTime check_out { get; set; }

    public decimal nightly_rate { get; set; }

    public decimal total_price { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<booking_history> booking_histories { get; set; } = new List<booking_history>();

    public virtual customer? customer { get; set; }

    public virtual room? room { get; set; }

    public virtual booking_status? status { get; set; }
}
