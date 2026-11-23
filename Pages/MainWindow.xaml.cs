using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ChudoObuv.Models;

namespace ChudoObuv
{
    public partial class MainWindow : Window
    {
        private List<CatalogItem> _all = new List<CatalogItem>();
        private bool _ready;

        public MainWindow()
        {
            InitializeComponent();
            UserFioText.Text = Session.IsGuest ? "Гость" : Session.UserFio;
            ApplyRoleRestrictions();
            LoadCategories();
            LoadCatalog();
            _ready = true;
            ApplyFilters();
        }

        /// <summary>
        /// Гость только просматривает каталог: корзина, поиск, фильтр
        /// и сортировка скрыты. Заказы доступны менеджеру и администратору.
        /// </summary>
        private void ApplyRoleRestrictions()
        {
            Visibility forAuthorized = Session.IsGuest
                ? Visibility.Collapsed
                : Visibility.Visible;

            CartButton.Visibility = forAuthorized;
            SearchBox.Visibility = forAuthorized;
            CategoryFilter.Visibility = forAuthorized;
            SortBox.Visibility = forAuthorized;
            OrdersButton.Visibility = Session.CanManageOrders
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void LoadCatalog()
        {
            try
            {
                _all = CatalogService.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить каталог.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_ready) ApplyFilters();
        }

        private void LoadCategories()
        {
            var list = new List<string>();
            try
            {
                using var db = new AppDbContext();
                list = db.Categories.OrderBy(c => c.Name).Select(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить список категорий.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            list.Insert(0, "Все категории");

            CategoryFilter.ItemsSource = list;
            CategoryFilter.SelectedIndex = 0;
        }

        private void Filters_Changed(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<CatalogItem> items = _all;

            if (CategoryFilter.SelectedIndex > 0)
            {
                string cat = (string)CategoryFilter.SelectedItem;
                items = items.Where(i => i.Category == cat);
            }

            string search = SearchBox.Text.Trim();
            if (search.Length > 0)
            {
                items = items.Where(i =>
                    i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    i.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            items = SortBox.SelectedIndex switch
            {
                1 => items.OrderBy(i => i.PriceWithDiscount),
                2 => items.OrderByDescending(i => i.PriceWithDiscount),
                _ => items
            };

            ProductsList.ItemsSource = items.ToList();
        }

        private void ProductItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Session.IsGuest
                && ((FrameworkElement)sender).DataContext is CatalogItem product)
            {
                new ProductWindow(product) { Owner = this }.ShowDialog();
                LoadCatalog();
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            var window = new CartWindow { Owner = this };
            if (window.ShowDialog() == true)
            {
                Session.Cart.Clear();
                LoadCatalog();
            }
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow { Owner = this }.ShowDialog();
            LoadCatalog();  
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Cart.Clear();
            Session.Reset();

            new LoginWindow().Show();
            Close();
        }
    }
}