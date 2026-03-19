using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Reference table for room availability states
/// </summary>
public partial class RoomStatus
{
    public byte Id { get; set; }

    public string StatusName { get; set; }

    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
