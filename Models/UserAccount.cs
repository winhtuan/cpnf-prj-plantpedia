using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class UserAccount
{
    public int UserId { get; set; }

    public string LastName { get; set; } = null!;

    public char Gender { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string AvatarUrl { get; set; } = null!;

    public virtual UserLoginDatum? UserLoginDatum { get; set; }
}
