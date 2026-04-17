using System;
using System.Collections.Generic;

namespace HotelCarga.HotelCarga.DbModel.Entities;

/// <summary>
/// User accounts with authentication and role assignment
/// </summary>
public partial class user
{
    public uint id { get; set; }

    public string username { get; set; } = string.Empty;

    public string email { get; set; } = string.Empty;

    public string password_hash { get; set; } = string.Empty;

    public byte role_id { get; set; }

    public byte status_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual customer? customer { get; set; }

    public virtual role? role { get; set; }

    public virtual user_status? status { get; set; }
}
