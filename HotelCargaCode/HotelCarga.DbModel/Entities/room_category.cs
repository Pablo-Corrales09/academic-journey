using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Reference table for room categories and types
/// </summary>
public partial class room_category
{
    public byte id { get; set; }

    public string category_name { get; set; }

    public string description { get; set; }

    public string amenities { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<room> rooms { get; set; } = new List<room>();

    public virtual ICollection<waiting_queue> waiting_queues { get; set; } = new List<waiting_queue>();
}
