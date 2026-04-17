using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Reference table for booking statuses
/// </summary>
public partial class booking_status
{
    public byte id { get; set; }

    public string status_name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime? created_at { get; set; }

    public virtual ICollection<booking_history> booking_histories { get; set; } = new List<booking_history>();

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();
}
