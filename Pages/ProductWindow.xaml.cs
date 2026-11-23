using System;
using System.Collections.Generic;
using System.Linq;
using ChudoObuv.Models;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace ChudoObuv
{
    internal class SizeOption
    {
        public int StockItemId { get; set; }
        public string Size { get; set; } = "";
        public int Quantity { get; set; }
    }

    public partial class ProductWindow : Window
    {
        private readonly CatalogItem _product;
        private readonly List<CartItem> _cart;   // корзина

        internal ProductWindow(CatalogItem product, List<CartItem>? cart = null)
        {
            InitializeComponent();
            _product = product;
            _cart = cart ?? Session.Cart;

            TitleText.Text = product.Manufacturer + " | " + product.Name;
            CategoryText.Text = "Категория: " + product.Category;
            ManufacturerText.Text = "Производство: " + product.Manufacturer;
            PriceText.Text = "Цена: " + product.PriceWithDiscount.ToString("0.00") + " ₽";
            CompositionText.Text = "Состав: " + product.Composition;
            DescriptionText.Text = "Описание: " + product.Description;
            ProductImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(product.Image));

            LoadSizes();
        }

        private void LoadSizes()
        {
            try
            {
                using var db = new AppDbContext();

                var all = db.StockItems
                    .Include(s => s.Size)
                    .Where(s => s.ProductID == _product.ProductId)
                    .OrderBy(s => s.Size.Value)
                    .ToList();

                // размерный ряд — все размеры модели, в списке — только доступные для заказа
                if (all.Count > 0)
                    SizeRangeText.Text = "Размерный ряд: " + all.First().Size.Value + " – " + all.Last().Size.Value;

                SizeBox.ItemsSource = all
                    .Where(s => s.Quantity > 0)
                    .Select(s => new SizeOption
                    {
                        StockItemId = s.StockItemID,
                        Size = s.Size.Value.ToString(),
                        Quantity = s.Quantity
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить размеры.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // в количестве пар можно только цифры
        private void QuantityBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !char.IsDigit(e.Text, 0);

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (SizeBox.SelectedItem is not SizeOption size)
            {
                MessageBox.Show("Выберите размер обуви.", "Оформление заказа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите количество пар — целое число больше нуля.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // учитываем то, что уже лежит в корзине
            var existing = _cart.FirstOrDefault(i => i.StockItemId == size.StockItemId);
            int inCart = existing?.Quantity ?? 0;

            if (qty + inCart > size.Quantity)
            {
                MessageBox.Show("Доступно только " + size.Quantity + " пар этого размера" +
                                (inCart > 0 ? " (в корзине уже " + inCart + ")." : "."),
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    StockItemId = size.StockItemId,
                    ProductName = TitleText.Text,
                    Size = size.Size,
                    Quantity = qty,
                    UnitPrice = _product.PriceWithDiscount   // цена со скидкой
                });
            }

            MessageBox.Show("Товарная позиция добавлена в заказ.", "Корзина",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e) => Close();
    }
}