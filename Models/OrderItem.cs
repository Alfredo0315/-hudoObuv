using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class OrderItem
{
    public int OrderItemID { get; set; }

    public int OrderID { get; set; }

    public int StockItemID { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual StockItem StockItem { get; set; } = null!;
}
