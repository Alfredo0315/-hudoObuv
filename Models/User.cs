using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class User
{
    public int UserID { get; set; }

    public string Login { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public int RoleID { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Role Role { get; set; } = null!;
}
