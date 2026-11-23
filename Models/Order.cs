using System;
using System.Collections.Generic;

namespace ChudoObuv.Models;

public partial class Order
{
    public int OrderID { get; set; }

    public DateOnly OrderDate { get; set; }

    public int UserID { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
