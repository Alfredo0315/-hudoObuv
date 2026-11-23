using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class StockItem
{
    public int StockItemID { get; set; }

    public int ProductID { get; set; }

    public int SizeID { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual Size Size { get; set; } = null!;
}
