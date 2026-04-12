using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

public partial class vw_customer_booking
{
    public uint customer_id { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string document_number { get; set; }

    public string phone { get; set; }

    public string email { get; set; }

    public string address { get; set; }

    public string city { get; set; }

    public string country { get; set; }

    public uint? booking_id { get; set; }

    public string reserve_number { get; set; }

    public DateTime? check_in { get; set; }

    public DateTime? check_out { get; set; }

    public decimal? nightly_rate { get; set; }

    public decimal? total_price { get; set; }

    public string booking_status { get; set; }

    public uint? room_number { get; set; }

    public byte? floor_number { get; set; }

    public string room_category { get; set; }

    public string room_status { get; set; }

    public DateTime? booking_created_at { get; set; }

    public DateTime? booking_updated_at { get; set; }
}
