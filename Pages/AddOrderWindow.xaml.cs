using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChudoObuv.Models;

namespace ChudoObuv
{
    internal class ClientOption
    {
        public int UserId { get; set; }
        public string Fio { get; set; } = "";
    }

    public partial class AddOrderWindow : Window
    {
        private List<CatalogItem> _catalog = new List<CatalogItem>();
        private readonly List<CartItem> _items = new List<CartItem>();

        public AddOrderWindow()
        {
            InitializeComponent();
            DateBox.SelectedDate = DateTime.Today;
            LoadClients();
            LoadCatalog();
            ShowItems();
        }

        private void LoadClients()
        {
            try
            {
                using var db = new AppDbContext();
                ClientBox.ItemsSource = db.Users
                    .OrderBy(u => u.LastName)
                    .ToList()
                    .Select(u => new ClientOption { UserId = u.UserID, Fio = OrderService.FullName(u) })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список клиентов.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCatalog()
        {
            try
            {
                _catalog = CatalogService.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить каталог.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ApplyFilter();
        }

        // поиск по наименованию
        private void ApplyFilter()
        {
            string search = SearchBox.Text.Trim();
            ProductsList.ItemsSource = _catalog
                .Where(i => search.Length == 0 || i.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductsList == null) return;
            ApplyFilter();
        }

        private void ProductItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is CatalogItem product)
            {
                new ProductWindow(product, _items) { Owner = this }.ShowDialog();
                ShowItems();
            }
        }

        private void ShowItems()
        {
            ItemsGrid.ItemsSource = _items.Select(i => new
            {
                i.ProductName, i.Size, i.Quantity, Sum = i.Quantity * i.UnitPrice
            }).ToList();
            TotalText.Text = "Итого: " + _items.Sum(i => i.Quantity * i.UnitPrice).ToString("0.00") + " ₽";
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            int index = ItemsGrid.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Выберите позицию для удаления.", "Новый заказ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _items.RemoveAt(index);
            ShowItems();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ClientBox.SelectedItem is not ClientOption client)
            {
                MessageBox.Show("Выберите клиента.", "Новый заказ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DateBox.SelectedDate is not DateTime date)
            {
                MessageBox.Show("Выберите дату заказа.", "Новый заказ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте в заказ хотя бы одну товарную позицию.", "Новый заказ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int orderId = OrderService.Create(client.UserId, DateOnly.FromDateTime(date), _items);

                MessageBox.Show("Заказ №" + orderId + " оформлен.", "Новый заказ",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось оформить заказ.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Close();
    }
}