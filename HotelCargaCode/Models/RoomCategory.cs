using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Reference table for room categories and types
/// </summary>
public partial class RoomCategory
{
    public byte Id { get; set; }

    public string CategoryName { get; set; }

    public string Description { get; set; }

    public string Amenities { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<WaitingQueue> WaitingQueues { get; set; } = new List<WaitingQueue>();
}
