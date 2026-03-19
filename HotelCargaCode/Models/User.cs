using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// User accounts with authentication and role assignment
/// </summary>
public partial class User
{
    public uint Id { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public byte RoleId { get; set; }

    public byte StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Role Role { get; set; }

    public virtual UserStatus Status { get; set; }
}
