using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Reference table for user roles
/// </summary>
public partial class Role
{
    public byte Id { get; set; }

    public string RoleName { get; set; }

    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
