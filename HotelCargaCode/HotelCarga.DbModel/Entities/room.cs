using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Physical rooms with pricing, category, and status information
/// </summary>
public partial class room
{
    public uint id { get; set; }

    public uint room_number { get; set; }

    public byte status_id { get; set; }

    public byte category_id { get; set; }

    public decimal nightly_rate { get; set; }

    public byte? floor_number { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<booking_history> booking_histories { get; set; } = new List<booking_history>();

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual room_category category { get; set; }

    public virtual ICollection<room_availability> room_availabilities { get; set; } = new List<room_availability>();

    public virtual room_status status { get; set; }
}
