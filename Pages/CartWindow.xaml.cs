using System;
using System.Linq;
using System.Windows;

namespace ChudoObuv
{
    public partial class CartWindow : Window
    {
        public CartWindow()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            CartGrid.ItemsSource = Session.Cart.Select(i => new
            {
                i.ProductName, i.Size, i.Quantity, i.UnitPrice, Sum = i.Quantity * i.UnitPrice
            }).ToList();
            TotalText.Text = "Итого: " + Session.Cart.Sum(i => i.Quantity * i.UnitPrice).ToString("0.00") + " ₽";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            int index = CartGrid.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Выберите позицию для удаления.", "Заказ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Session.Cart.RemoveAt(index);
            Refresh();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (Session.Cart.Count == 0)
            {
                MessageBox.Show("Заказ пуст. Добавьте хотя бы одну товарную позицию.",
                    "Заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int orderId = OrderService.Create(
                    Session.UserId, DateOnly.FromDateTime(DateTime.Today), Session.Cart);
                Session.Cart.Clear();

                MessageBox.Show("Заказ №" + orderId + " оформлен.",
                    "Заказ", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;      // перезагрузка каталога
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