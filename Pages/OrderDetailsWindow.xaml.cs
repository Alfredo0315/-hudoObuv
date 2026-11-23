using System;
using System.Linq;
using System.Windows;
using ChudoObuv.Models;
using Microsoft.EntityFrameworkCore;

namespace ChudoObuv
{
    internal class OrderItemRow
    {
        public int OrderItemId { get; set; }
        public string Product { get; set; } = "";
        public string Size { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Sum => Quantity * UnitPrice;
    }

    public partial class OrderDetailsWindow : Window
    {
        private readonly int _orderId;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;

            // менеджер только просматривает заказ, администратор редактирует
            DateBox.IsEnabled = Session.IsAdmin;
            SaveDateButton.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            RemoveItemButton.Visibility = Session.IsAdmin ? Visibility.Visible : Visibility.Collapsed;

            LoadHeader();
            LoadItems();
        }

        private void LoadHeader()
        {
            try
            {
                using var db = new AppDbContext();
                var order = db.Orders.Include(o => o.User).First(o => o.OrderID == _orderId);

                HeaderText.Text = "Заказ №" + order.OrderID;
                ClientText.Text = "Клиент: " + OrderService.FullName(order.User);
                DateBox.SelectedDate = order.OrderDate.ToDateTime(TimeOnly.MinValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить заказ.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadItems()
        {
            try
            {
                using var db = new AppDbContext();

                var rows = db.OrderItems
                    .Include(oi => oi.StockItem).ThenInclude(s => s.Product).ThenInclude(p => p.Manufacturer)
                    .Include(oi => oi.StockItem).ThenInclude(s => s.Size)
                    .Where(oi => oi.OrderID == _orderId)
                    .ToList()
                    .Select(oi => new OrderItemRow
                    {
                        OrderItemId = oi.OrderItemID,
                        Product = oi.StockItem.Product.Manufacturer?.Name + " | " + oi.StockItem.Product.Name,
                        Size = oi.StockItem.Size.Value.ToString(),
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    })
                    .ToList();

                ItemsGrid.ItemsSource = rows;
                TotalText.Text = "Итого: " + rows.Sum(r => r.Sum).ToString("0.00") + " ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить состав заказа.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveDate_Click(object sender, RoutedEventArgs e)
        {
            if (DateBox.SelectedDate is not DateTime date)
            {
                MessageBox.Show("Выберите дату заказа.", "Редактирование заказа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();
                db.Orders.First(o => o.OrderID == _orderId).OrderDate = DateOnly.FromDateTime(date);
                db.SaveChanges();

                MessageBox.Show("Дата заказа изменена.", "Редактирование заказа",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось изменить дату.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsGrid.SelectedItem is not OrderItemRow row)
            {
                MessageBox.Show("Выберите позицию для удаления.", "Редактирование заказа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show(
                "Удалить позицию «" + row.Product + "», размер " + row.Size + "?\n" +
                "Товар вернётся на склад. Это действие нельзя отменить.",
                "Удаление позиции", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            try
            {
                using var db = new AppDbContext();
                var item = db.OrderItems
                    .Include(oi => oi.StockItem)
                    .First(oi => oi.OrderItemID == row.OrderItemId);

                item.StockItem.Quantity += item.Quantity;
                db.OrderItems.Remove(item);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить позицию.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadItems();
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Close();
    }
}