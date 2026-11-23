using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Manufacturer
{
    public int ManufacturerID { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
