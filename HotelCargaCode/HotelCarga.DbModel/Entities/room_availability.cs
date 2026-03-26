using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Room availability schedule with date range management
/// </summary>
public partial class room_availability
{
    public uint id { get; set; }

    public uint room_id { get; set; }

    public DateTime start_schedule { get; set; }

    public DateTime end_schedule { get; set; }

    public DateTime? creation_at { get; set; }

    public virtual room room { get; set; }
}
