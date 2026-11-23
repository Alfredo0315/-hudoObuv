using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Subcategory
{
    public int SubcategoryID { get; set; }

    public int CategoryID { get; set; }

    public string Name { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
