using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

public partial class vw_waiting_queue_report
{
    public uint queue_id { get; set; }

    public uint customer_id { get; set; }

    public string first_name { get; set; } = string.Empty;

    public string last_name { get; set; } = string.Empty;

    public string document_number { get; set; } = string.Empty;

    public string requested_room_category { get; set; } = string.Empty;

    public DateTime requested_check_in { get; set; }

    public DateTime? check_out { get; set; }

    public string queue_status { get; set; } = string.Empty;

    public string current_status { get; set; } = string.Empty;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
