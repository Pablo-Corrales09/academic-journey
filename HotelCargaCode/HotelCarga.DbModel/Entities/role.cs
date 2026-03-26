using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// Reference table for user roles
/// </summary>
public partial class role
{
    public byte id { get; set; }

    public string role_name { get; set; }

    public string description { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
