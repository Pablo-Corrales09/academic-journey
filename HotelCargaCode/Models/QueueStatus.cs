using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Reference table for waiting queue entry statuses
/// </summary>
public partial class QueueStatus
{
    public byte Id { get; set; }

    public string StatusName { get; set; }

    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<WaitingQueue> WaitingQueues { get; set; } = new List<WaitingQueue>();
}
