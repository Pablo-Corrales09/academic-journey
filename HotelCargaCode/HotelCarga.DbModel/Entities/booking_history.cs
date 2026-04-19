using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Audit trail tracking all booking changes and historical events
/// </summary>
public partial class booking_history
{
    public uint id { get; set; }

    public uint booking_id { get; set; }

    public uint customer_id { get; set; }

    public uint? room_id { get; set; }

    public DateTime check_in { get; set; }

    public DateTime check_out { get; set; }

    public byte status_id { get; set; }

    public string action_type { get; set; } = string.Empty;

    public decimal? nightly_rate { get; set; }

    public decimal? total_price { get; set; }

    public DateTime? created_at { get; set; }

    public virtual booking booking { get; set; } = null!;

    public virtual customer customer { get; set; } = null!;

    public virtual room? room { get; set; }

    public virtual booking_status status { get; set; } = null!;
}
