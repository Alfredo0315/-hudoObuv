using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Size
{
    public int SizeID { get; set; }

    public decimal Value { get; set; }

    public virtual ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
