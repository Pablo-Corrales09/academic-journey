using System;
using System.Collections.Generic;

namespace HotelCarga.Models;

/// <summary>
/// Reference table for user account statuses
/// </summary>
public partial class UserStatus
{
    public byte Id { get; set; }

    public string StatusName { get; set; }

    public string Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
