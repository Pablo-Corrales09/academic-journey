using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Reference table for waiting queue entry statuses
/// </summary>
public partial class queue_status
{
    public byte id { get; set; }

    public string status_name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime? created_at { get; set; }

    public virtual ICollection<waiting_queue> waiting_queues { get; set; } = new List<waiting_queue>();
}
