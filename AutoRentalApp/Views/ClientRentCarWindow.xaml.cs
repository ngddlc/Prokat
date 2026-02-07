using AutoRentalApp.Data;
using AutoRentalApp.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;

namespace AutoRentalApp.Views
{
    public partial class ClientRentCarWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly Client _currentClient;
        private readonly Car _selectedCar;
        private decimal _dailyPrice;
        private int _daysCount;

        public ClientRentCarWindow(AppDbContext dbContext, Client currentClient, Car selectedCar)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _currentClient = currentClient;
            _selectedCar = selectedCar;

            // Отображение информации об автомобиле
            CarInfoText.Text = $"{selectedCar.Brand} {selectedCar.Model} ({selectedCar.PlateNumber})\n" +
                              $"Год: {selectedCar.Year}, Цвет: {selectedCar.Color}\n" +
                              $"Класс: {selectedCar.CarClass}, Кузов: {selectedCar.BodyType}\n" +
                              $"Двигатель: {selectedCar.EngineType} {selectedCar.EngineVolume}л, КПП: {selectedCar.TransmissionType}";

            _dailyPrice = selectedCar.DailyPrice;
            DailyPriceText.Text = $"{_dailyPrice:N2} руб.";

            // Установка дат по умолчанию
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(3);

            CalculateTotal();
        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                var startDate = StartDatePicker.SelectedDate.Value.Date;
                var endDate = EndDatePicker.SelectedDate.Value.Date;

                if (endDate <= startDate)
                {
                    MessageBox.Show("Дата возврата должна быть позже даты выдачи", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _daysCount = (endDate - startDate).Days;
                DaysCountText.Text = $"{_daysCount} дней";

                decimal total = _dailyPrice * _daysCount;
                TotalAmountText.Text = $"{total:N2} руб.";
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (!StartDatePicker.SelectedDate.HasValue)
            {
                StartDateError.Text = "Выберите дату выдачи";
                StartDateError.Visibility = Visibility.Visible;
                return;
            }

            if (!EndDatePicker.SelectedDate.HasValue)
            {
                EndDateError.Text = "Выберите дату возврата";
                EndDateError.Visibility = Visibility.Visible;
                return;
            }

            if (EndDatePicker.SelectedDate <= StartDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата возврата должна быть позже даты выдачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка доступности автомобиля на выбранные даты
            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            var conflictingContracts = _dbContext.RentalContracts
                .Where(rc => rc.CarID == _selectedCar.CarID && rc.ContractStatusID != 3)
                .Any(rc =>
                    (startDate >= rc.StartDateTime && startDate < rc.PlannedEndDateTime) ||
                    (endDate > rc.StartDateTime && endDate <= rc.PlannedEndDateTime) ||
                    (startDate <= rc.StartDateTime && endDate >= rc.PlannedEndDateTime)
                );

            if (conflictingContracts)
            {
                MessageBox.Show("К сожалению, автомобиль уже забронирован на выбранные даты. Пожалуйста, выберите другие даты или другой автомобиль.",
                    "Автомобиль недоступен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаем фейкового менеджера для заглушки
            var manager = _dbContext.Employees.FirstOrDefault(emp => emp.Position.Contains("менеджер"));

            if (manager == null)
            {
                // Ищем администратора (RoleID = 1)
                var adminUser = _dbContext.Users.FirstOrDefault(u => u.RoleID == 1);

                // Если администратора нет, создаем фейкового Заглушка
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        FirstName = "Система",
                        LastName = "Администратор",
                        Login = "system_manager",
                        PasswordHash = "fake_hash",
                        RoleID = 1
                    };
                    _dbContext.Users.Add(adminUser);
                    _dbContext.SaveChanges();
                }

                // Создаем фейкового менеджера, привязанного к администратору
                manager = new Employee
                {
                    UserID = adminUser.UserID,
                    Position = "менеджер",
                    HireDate = DateTime.Today
                };
                _dbContext.Employees.Add(manager);
                _dbContext.SaveChanges();
            }

            // Создание договора аренды
            try
            {
                var contract = new RentalContract
                {
                    ClientID = _currentClient.ClientID,
                    CarID = _selectedCar.CarID,
                    ManagerID = manager.EmployeeID,
                    StartDateTime = startDate,  
                    PlannedEndDateTime = endDate,
                    ActualEndDateTime = null,
                    ContractStatusID = 1,
                    TotalAmount = _dailyPrice * _daysCount
                };

                _dbContext.RentalContracts.Add(contract);
                _dbContext.SaveChanges();

                // Обновляем статус автомобиля
                _selectedCar.CarStatusID = 2;
                _dbContext.SaveChanges();

                MessageBox.Show($"Договор аренды успешно оформлен!\nНомер договора: ДОГ-{contract.ContractID}\n\n" +
                               $"Пожалуйста, обратитесь в офис для подписания договора и получения автомобиля.",
                               "Аренда оформлена", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении аренды: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}