using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Waiting queue for customers seeking room availability
/// </summary>
public partial class waiting_queue
{
    public uint id { get; set; }

    public uint customer_id { get; set; }

    public byte room_category_id { get; set; }

    public byte status_id { get; set; }

    public DateTime requested_check_in { get; set; }

    public DateTime? check_out { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual customer customer { get; set; }

    public virtual room_category room_category { get; set; }

    public virtual queue_status status { get; set; }
}
