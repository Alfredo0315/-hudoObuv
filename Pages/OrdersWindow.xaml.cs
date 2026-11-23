using System;
using System.Linq;
using System.Windows;
using ChudoObuv.Models;
using Microsoft.EntityFrameworkCore;

namespace ChudoObuv
{
    internal class OrderRow
    {
        public int OrderId { get; set; }
        public string Date { get; set; } = "";
        public string Client { get; set; } = "";
        public decimal Total { get; set; }
    }

    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using var db = new AppDbContext();

                var totals = db.OrderItems
                    .GroupBy(oi => oi.OrderID)
                    .Select(g => new { OrderId = g.Key, Sum = g.Sum(x => x.Quantity * x.UnitPrice) })
                    .ToDictionary(x => x.OrderId, x => x.Sum);

                OrdersGrid.ItemsSource = db.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList()
                    .Select(o => new OrderRow
                    {
                        OrderId = o.OrderID,
                        Date = o.OrderDate.ToString("dd.MM.yyyy"),
                        Client = OrderService.FullName(o.User),
                        Total = totals.GetValueOrDefault(o.OrderID)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список заказов.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private OrderRow? GetSelected()
        {
            if (OrdersGrid.SelectedItem is OrderRow row) return row;

            MessageBox.Show("Выберите заказ в списке.", "Заказы",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        private void OpenDetails(OrderRow row)
        {
            new OrderDetailsWindow(row.OrderId) { Owner = this }.ShowDialog();
            LoadOrders();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var row = GetSelected();
            if (row != null) OpenDetails(row);
        }

        private void OrdersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (OrdersGrid.SelectedItem is OrderRow row) OpenDetails(row);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (new AddOrderWindow { Owner = this }.ShowDialog() == true)
                LoadOrders();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var row = GetSelected();
            if (row == null) return;

            var answer = MessageBox.Show(
                "Удалить заказ №" + row.OrderId + " (" + row.Client + ")?\n" +
                "Товары из заказа вернутся на склад. Это действие нельзя отменить.",
                "Удаление заказа", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var db = new AppDbContext();
                using var transaction = db.Database.BeginTransaction();

                var items = db.OrderItems
                    .Include(oi => oi.StockItem)
                    .Where(oi => oi.OrderID == row.OrderId)
                    .ToList();

               
                foreach (var item in items)
                    item.StockItem.Quantity += item.Quantity;

                db.OrderItems.RemoveRange(items);
                db.Orders.Remove(db.Orders.First(o => o.OrderID == row.OrderId));
                db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить заказ.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadOrders();
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Close();
    }
}