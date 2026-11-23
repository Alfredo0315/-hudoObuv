using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Product
{
    public int ProductID { get; set; }

    public string Name { get; set; } = null!;

    public int SubcategoryID { get; set; }

    public int ManufacturerID { get; set; }

    public string? Description { get; set; }

    public string? Composition { get; set; }

    public string? Image { get; set; }

    public decimal Price { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();

    public virtual Subcategory Subcategory { get; set; } = null!;
}
