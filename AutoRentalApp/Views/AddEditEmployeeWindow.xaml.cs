using AutoRentalApp.Data;
using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class AddEditEmployeeWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly Employee _employeeToEdit;
        private User _userToEdit;

        public AddEditEmployeeWindow(AppDbContext dbContext, Employee employee)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _employeeToEdit = employee;

            // Загрузка ролей в выпадающий список (только менеджер и администратор)
            var roles = _dbContext.Roles
                .Where(r => r.RoleName == "менеджер" || r.RoleName == "администратор")
                .ToList();

            foreach (var role in roles)
            {
                RoleComboBox.Items.Add(new ComboBoxItem { Content = role.RoleName, Tag = role.RoleID });
            }

            if (employee != null)
            {
                // Режим редактирования
                Title = "Редактирование сотрудника";
                TitleText.Text = "Редактирование сотрудника";
                LoadEmployeeData();
                PasswordBox.Visibility = Visibility.Collapsed;
                var passLabel = new TextBlock
                {
                    Text = "Пароль: ****** (скрыт)",
                    Style = this.FindResource("FormLabel") as Style,
                    Margin = new Thickness(0, 8, 0, 4)
                };
                var stackPanel = (StackPanel)TitleText.Parent;
                stackPanel.Children.Insert(6, passLabel);
            }
            else
            {
                // Установка роли по умолчанию для нового сотрудника
                if (RoleComboBox.Items.Count > 0)
                    RoleComboBox.SelectedIndex = 0;
            }
        }

        private void LoadEmployeeData()
        {
            _userToEdit = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserID == _employeeToEdit.UserID);

            if (_userToEdit == null)
            {
                MessageBox.Show("Ошибка: пользователь сотрудника не найден", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LastNameBox.Text = _userToEdit.LastName;
            FirstNameBox.Text = _userToEdit.FirstName;
            LoginBox.Text = _userToEdit.Login;
            LoginBox.IsEnabled = false; // Логин нельзя изменить

            PositionBox.Text = _employeeToEdit.Position;
            HireDatePicker.SelectedDate = _employeeToEdit.HireDate;

            // Выбор роли
            var roleItem = RoleComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => (int)item.Tag == _userToEdit.RoleID);
            if (roleItem != null)
                RoleComboBox.SelectedItem = roleItem;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка ошибок
            LastNameError.Visibility = Visibility.Collapsed;
            FirstNameError.Visibility = Visibility.Collapsed;
            LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            PositionError.Visibility = Visibility.Collapsed;
            HireDateError.Visibility = Visibility.Collapsed;

            // Валидация
            bool isValid = true;
            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string position = PositionBox.Text.Trim();
            DateTime? hireDate = HireDatePicker.SelectedDate;
            var selectedRole = RoleComboBox.SelectedItem as ComboBoxItem;

            if (string.IsNullOrWhiteSpace(lastName))
            {
                LastNameError.Text = "Фамилия обязательна";
                LastNameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                FirstNameError.Text = "Имя обязательно";
                FirstNameError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(login))
            {
                LoginError.Text = "Логин обязателен";
                LoginError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (_employeeToEdit == null && string.IsNullOrWhiteSpace(password))
            {
                PasswordError.Text = "Пароль обязателен при создании";
                PasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(position))
            {
                PositionError.Text = "Должность обязательна";
                PositionError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!hireDate.HasValue)
            {
                HireDateError.Text = "Выберите дату приёма";
                HireDateError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (selectedRole == null)
            {
                MessageBox.Show("Выберите роль сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!isValid)
                return;

            try
            {
                if (_employeeToEdit == null)
                {
                    // Проверка уникальности логина
                    if (_dbContext.Users.Any(u => u.Login == login))
                    {
                        LoginError.Text = "Логин уже занят";
                        LoginError.Visibility = Visibility.Visible;
                        return;
                    }

                    string passwordHash = HashPassword(password);

                    var newUser = new User
                    {
                        LastName = lastName,
                        FirstName = firstName,
                        Login = login,
                        PasswordHash = passwordHash,
                        RoleID = (int)selectedRole.Tag
                    };

                    _dbContext.Users.Add(newUser);
                    _dbContext.SaveChanges();

                    var newEmployee = new Employee
                    {
                        UserID = newUser.UserID,
                        Position = position,
                        HireDate = hireDate.Value
                    };

                    _dbContext.Employees.Add(newEmployee);
                }
                else
                {
                    // Редактирование существующего сотрудника
                    _userToEdit.LastName = lastName;
                    _userToEdit.FirstName = firstName;

                    _employeeToEdit.Position = position;
                    _employeeToEdit.HireDate = hireDate.Value;

                    // Обновление роли 
                    if (selectedRole != null && (int)selectedRole.Tag != _userToEdit.RoleID)
                    {
                        _userToEdit.RoleID = (int)selectedRole.Tag;
                    }
                }

                _dbContext.SaveChanges();
                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}