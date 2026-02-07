using AutoRentalApp.Data;
using AutoRentalApp.Helpers;
using AutoRentalApp.Services;
using AutoRentalApp.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _dbContext;

        public LoginWindow()
        {
            InitializeComponent();

            string connectionString = DbHelper.GetConnectionString();
            _dbContext = new AppDbContext(connectionString);
            _authService = new AuthService(_dbContext);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;

            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                LoginErrorText.Text = "Введите логин";
                LoginErrorText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordErrorText.Text = "Введите пароль";
                PasswordErrorText.Visibility = Visibility.Visible;
                return;
            }

            var result = _authService.Login(login, password);

            if (result.success)
            {
                MessageBox.Show($"Добро пожаловать, {result.user.FullName}!\nРоль: {result.user.Role?.RoleName}",
                    "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                OpenMainWindow(result.user);
            }
            else
            {
                MessageBox.Show(result.message, "Ошибка авторизации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PasswordBox.Clear();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
            LoginBox.Clear();
            PasswordBox.Clear();
        }

        private void OpenMainWindow(AutoRentalApp.Models.User user)
        {
            try
            {
                Window mainWindow = null;

                switch (user.Role?.RoleName)
                {
                    case "администратор":
                        mainWindow = new AdminWindow(_authService, _dbContext);
                        break;

                    case "менеджер":
                        // Заглушка для менеджера
                        mainWindow = CreatePlaceholderWindow(user.FullName, "Панель менеджера");
                        break;

                    case "клиент":
                        // Проверяем наличие клиента ДО создания окна
                        var clientExists = _dbContext.Clients.Any(c => c.UserID == user.UserID);

                        if (!clientExists)
                        {
                            MessageBox.Show(
                                "Ошибка: данные клиента не найдены в системе.\n" +
                                "Обратитесь к администратору для завершения регистрации.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        mainWindow = new ClientWindow(_authService, _dbContext);
                        break;

                    default:
                        mainWindow = CreatePlaceholderWindow(user.FullName, "Главное окно");
                        break;
                }

                if (mainWindow != null)
                {
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Window CreatePlaceholderWindow(string userName, string title)
        {
            var window = new Window
            {
                Title = $"Авто в прокат — {title}",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = System.Windows.Media.Brushes.White
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Шапка
            var header = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2196F3")),
                Padding = new Thickness(20, 0, 20, 0)
            };
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = "🚗 ",
                FontSize = 24,
                Foreground = System.Windows.Media.Brushes.White
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{title} | {userName}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });
            header.Child = headerPanel;
            Grid.SetRow(header, 0);
            grid.Children.Add(header);

            // Контент
            var content = new TextBlock
            {
                Text = $"Добро пожаловать, {userName}!\n\nРоль: {title}\n\nПолноценный интерфейс находится в разработке.",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
            Grid.SetRow(content, 1);
            grid.Children.Add(content);

            window.Content = grid;
            return window;
        }
    }
}