using System;
using System.Collections.Generic;
using System.Linq;
using ChudoObuv.Models;

namespace ChudoObuv
{
    internal static class OrderService
    {
        public static string FullName(User? user)
            => user == null ? "" : (user.LastName + " " + user.FirstName + " " + user.Patronymic).Trim();

        // создаёт заказ и списывает товар со склада 
        public static int Create(int userId, DateOnly date, IEnumerable<CartItem> items)
        {
            using var db = new AppDbContext();
            using var transaction = db.Database.BeginTransaction();

            var order = new Order { OrderDate = date, UserID = userId };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (CartItem item in items)
            {
                var stock = db.StockItems.First(s => s.StockItemID == item.StockItemId);
                if (stock.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        "Недостаточно товара на складе: " + item.ProductName + ", размер " + item.Size);

                stock.Quantity -= item.Quantity;

                db.OrderItems.Add(new OrderItem
                {
                    OrderID = order.OrderID,
                    StockItemID = item.StockItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            db.SaveChanges();
            transaction.Commit();
            return order.OrderID;
        }
    }
}
