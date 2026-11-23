using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Category
{
    public int CategoryID { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
}
