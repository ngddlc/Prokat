using AutoRentalApp.Data;
using AutoRentalApp.Models;
using AutoRentalApp.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class ClientWindow : Window
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _dbContext;
        private readonly Client _currentClient;
        private readonly User _currentUser;

        // Вспомогательный класс для отображения договоров
        private class ContractDisplayItem
        {
            public int ContractID { get; set; }
            public string ContractNumber { get; set; }
            public string CarDisplayName { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime PlannedEndDateTime { get; set; }
            public DateTime? ActualEndDateTime { get; set; }
            public ContractStatus ContractStatus { get; set; }
            public decimal TotalAmount { get; set; }
            public int CarID { get; set; }
            public int CarStatusID { get; set; }
        }

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

                // ИСПРАВЛЕНО: Проверка и создание клиента, если его нет
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

                // Автоматическое обновление каждые 30 секунд
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(30);
                timer.Tick += (s, e) => LoadContracts();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка инициализации личного кабинета:\n\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.Dispatcher.BeginInvoke(new Action(() => this.Close()));
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
                // ИСПРАВЛЕНО: Полная загрузка с включением ВСЕХ связанных данных
                var contracts = _dbContext.RentalContracts
                    .Include(rc => rc.Car)
                        .ThenInclude(c => c.CarStatus)
                    .Include(rc => rc.ContractStatus)
                    .Include(rc => rc.Manager)
                        .ThenInclude(m => m.User)
                    .Where(rc => rc.ClientID == _currentClient.ClientID)
                    .OrderByDescending(rc => rc.StartDateTime)
                    .ToList();

                // Формируем отображаемые данные с безопасной проверкой
                var displayItems = new List<ContractDisplayItem>();

                foreach (var contract in contracts)
                {
                    string carDisplayName = "Автомобиль недоступен";

                    if (contract.Car != null)
                    {
                        // Безопасное получение названия автомобиля
                        carDisplayName = !string.IsNullOrWhiteSpace(contract.Car.DisplayName)
                            ? contract.Car.DisplayName
                            : $"{contract.Car.Brand} {contract.Car.Model}";
                    }

                    displayItems.Add(new ContractDisplayItem
                    {
                        ContractID = contract.ContractID,
                        ContractNumber = contract.ContractNumber,
                        CarDisplayName = carDisplayName,
                        StartDateTime = contract.StartDateTime,
                        PlannedEndDateTime = contract.PlannedEndDateTime,
                        ActualEndDateTime = contract.ActualEndDateTime,
                        ContractStatus = contract.ContractStatus,
                        TotalAmount = contract.TotalAmount,
                        CarID = contract.Car?.CarID ?? 0,
                        CarStatusID = contract.Car?.CarStatusID ?? 0
                    });
                }

                ContractsDataGrid.ItemsSource = displayItems;
                // УБРАНО: StatusText.Text = ... (элемента нет в XAML)
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка загрузки договоров:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ContractsDataGrid.ItemsSource = new List<ContractDisplayItem>();
                // УБРАНО: StatusText.Text = ... (элемента нет в XAML)
            }
        }

        private void DeleteContractButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as ContractDisplayItem;

            if (item == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись аренды?\n\n" +
                $"Автомобиль: {item.CarDisplayName}\n" +
                $"Период: {item.StartDateTime:d} - {item.PlannedEndDateTime:d}\n\n" +
                "⚠️ ВНИМАНИЕ: Это действие вернёт автомобиль в статус 'свободен' " +
                "и удалит запись из вашей истории аренды.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Находим договор в БД
                    var contract = _dbContext.RentalContracts
                        .Include(rc => rc.Car)
                        .FirstOrDefault(rc => rc.ContractID == item.ContractID);

                    if (contract == null)
                    {
                        MessageBox.Show("Договор не найден в базе данных", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обновляем статус автомобиля на "свободен" (1)
                    if (contract.Car != null && contract.Car.CarStatusID != 1)
                    {
                        contract.Car.CarStatusID = 1;
                    }

                    // Удаляем договор
                    _dbContext.RentalContracts.Remove(contract);
                    _dbContext.SaveChanges();

                    // Обновляем список
                    LoadContracts();

                    MessageBox.Show(
                        $"Запись аренды успешно удалена.\n" +
                        $"Автомобиль '{item.CarDisplayName}' теперь свободен для аренды.",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении записи:\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
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