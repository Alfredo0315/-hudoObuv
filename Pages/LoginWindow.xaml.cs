using System;
using System.Linq;
using System.Windows;
using ChudoObuv.Models;
using Microsoft.EntityFrameworkCore;

namespace ChudoObuv
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoadLogins();
        }

        private void LoadLogins()
        {
            try
            {
                using var db = new AppDbContext();
                LoginBox.ItemsSource = db.Users.OrderBy(u => u.Login).Select(u => u.Login).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключиться к базе данных.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();

            if (login.Length == 0)
            {
                MessageBox.Show("Введите логин.", "Ошибка авторизации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var user = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    MessageBox.Show(
                        "Пользователь с логином «" + login + "» не найден.\n" +
                        "Введите логин из списка пользователей системы.",
                        "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Session.UserId = user.UserID;
                Session.UserFio = (user.LastName + " " + user.FirstName + " " + user.Patronymic).Trim();
                Session.RoleName = user.Role?.Name ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить вход.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenCatalog();
        }

        private void OpenCatalog()
        {
            new MainWindow().Show();
            Close();
        }
    }
}