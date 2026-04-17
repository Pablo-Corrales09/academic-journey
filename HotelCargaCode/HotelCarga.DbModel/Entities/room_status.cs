using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Reference table for room availability states
/// </summary>
public partial class room_status
{
    public byte id { get; set; }

    public string status_name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime? created_at { get; set; }

    public virtual ICollection<room> rooms { get; set; } = new List<room>();
}
