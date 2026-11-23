using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Role
{
    public int RoleID { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
