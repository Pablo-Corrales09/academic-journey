using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Waiting queue for customers seeking room availability
/// </summary>
public partial class WaitingQueue
{
    public uint Id { get; set; }

    public uint CustomerId { get; set; }

    public byte RoomCategoryId { get; set; }

    public byte StatusId { get; set; }

    public DateTime RequestedCheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual RoomCategory RoomCategory { get; set; }

    public virtual QueueStatus Status { get; set; }
}
