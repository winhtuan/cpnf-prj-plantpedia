using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class UserLoginDatum
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
