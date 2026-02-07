using AutoRentalApp.Data;
using AutoRentalApp.Models;
using AutoRentalApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace AutoRentalApp.Views
{
    public partial class ClientWindow : Window
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _dbContext;
        private readonly Client _currentClient;
        private readonly User _currentUser;

        public ClientWindow(AuthService authService, AppDbContext dbContext)
        {
            InitializeComponent();
            _authService = authService;
            _dbContext = dbContext;

            try
            {
                _currentUser = _authService.GetCurrentUser();
                if (_currentUser == null)
                {
                    throw new Exception("Пользователь не авторизован. Пожалуйста, войдите снова.");
                }

                UserNameText.Text = $"Здравствуйте, {_currentUser.FullName}";

                _currentClient = _dbContext.Clients
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.UserID == _currentUser.UserID);

                // Если клиента нет, но роль клиента - создаем запись
                if (_currentClient == null && _currentUser.RoleID == 3)
                {
                    _currentClient = new Client
                    {
                        UserID = _currentUser.UserID,
                        PassportNumber = "Не указан",
                        DriverLicenseNumber = "Не указан",
                        Phone = "Не указан",
                        Email = null
                    };

                    _dbContext.Clients.Add(_currentClient);
                    _dbContext.SaveChanges();

                    MessageBox.Show(
                        "Данные клиента были автоматически созданы.\n" +
                        "Пожалуйста, заполните профиль в разделе 'Редактировать профиль'.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                if (_currentClient == null)
                {
                    throw new Exception(
                        "Данные клиента не найдены в системе.\n\n" +
                        "Возможные причины:\n" +
                        "• Регистрация не завершена корректно\n" +
                        "• Учётная запись удалена администратором"
                    );
                }

                LoadClientInfo();
                LoadContracts();
            }
            catch (Exception ex)
            {
                // Показываем ошибку пользователю
                MessageBox.Show(
                    $"Ошибка инициализации личного кабинета:\n\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Close();
                }));
            }
        }

        private void LoadClientInfo()
        {
            if (_currentClient == null || _currentClient.User == null)
                return;

            FullNameText.Text = $"{_currentClient.User.LastName} {_currentClient.User.FirstName}";
            LoginText.Text = _currentClient.User.Login;
            PassportText.Text = _currentClient.PassportNumber;
            LicenseText.Text = _currentClient.DriverLicenseNumber;
            PhoneText.Text = _currentClient.Phone;
            EmailText.Text = _currentClient.Email ?? "Не указан";
        }

        private void LoadContracts()
        {
            try
            {
                // Безопасная загрузка с проверкой соединения
                var contracts = _dbContext.RentalContracts
                    .Include(rc => rc.Car)
                        .ThenInclude(c => c.CarStatus)
                    .Include(rc => rc.ContractStatus)
                    .Where(rc => rc.ClientID == _currentClient.ClientID)
                    .OrderByDescending(rc => rc.StartDateTime)
                    .ToList();

                // Создаём список для отображения
                var displayContracts = contracts.Select(c => new
                {
                    c.ContractNumber,
                    c.Car.DisplayName,
                    c.StartDateTime,
                    c.PlannedEndDateTime,
                    c.ActualEndDateTime,
                    c.ContractStatus.StatusName,
                    c.TotalAmount
                }).ToList();

                ContractsDataGrid.ItemsSource = displayContracts;
            }
            catch (Exception ex)
            {
                // Безопасная обработка ошибки
                MessageBox.Show(
                    $"Ошибка загрузки договоров:\n\n{ex.Message}\n\n" +
                    "Попробуйте обновить страницу или обратитесь к администратору.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Close();
                }));
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            new LoginWindow().Show();
            this.Close();
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddEditClientWindow(_dbContext, _currentClient);
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                _currentClient.User = _dbContext.Users.Find(_currentClient.UserID);
                LoadClientInfo();
            }
        }

        private void NewRentalButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем каталог автомобилей
            var catalogWindow = new ClientCarsCatalog(_dbContext, _currentClient);
            catalogWindow.ShowDialog();

            // Обновляем список договоров после аренды
            LoadContracts();
        }
    }
}