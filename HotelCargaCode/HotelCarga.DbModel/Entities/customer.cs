using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Customer profiles with personal identification and contact information
/// </summary>
public partial class customer
{
    public uint id { get; set; }

    public uint user_id { get; set; }

    public string document_number { get; set; } = string.Empty;

    public string first_name { get; set; } = string.Empty;

    public string last_name { get; set; } = string.Empty;

    public string phone { get; set; } = string.Empty;

    public string address { get; set; } = string.Empty;

    public string city { get; set; } = string.Empty;

    public string country { get; set; } = string.Empty;

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<booking_history> booking_histories { get; set; } = new List<booking_history>();

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual user? user { get; set; }

    public virtual ICollection<waiting_queue> waiting_queues { get; set; } = new List<waiting_queue>();
}
