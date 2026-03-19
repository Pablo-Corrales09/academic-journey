using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Room availability schedule with date range management
/// </summary>
public partial class RoomAvailability
{
    public uint Id { get; set; }

    public uint RoomId { get; set; }

    public DateTime StartSchedule { get; set; }

    public DateTime EndSchedule { get; set; }

    public DateTime? CreationAt { get; set; }

    public virtual Room Room { get; set; }
}
