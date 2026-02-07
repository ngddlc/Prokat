using AutoRentalApp.Data;
using AutoRentalApp.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class AddEditClientWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly Client _clientToEdit;
        private User _userToEdit;

        public AddEditClientWindow(AppDbContext dbContext, Client client)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _clientToEdit = client;

            if (client != null)
            {
                // Режим редактирования
                Title = "Редактирование клиента";
                TitleText.Text = "Редактирование клиента";
                LoadClientData();
                PasswordBox.Visibility = Visibility.Collapsed;
                var passLabel = new TextBlock
                {
                    Text = "Пароль: ****** (скрыт)",
                    Style = this.FindResource("FormLabel") as Style,
                    Margin = new Thickness(0, 8, 0, 4)
                };
                var stackPanel = (StackPanel)TitleText.Parent;
                stackPanel.Children.Insert(7, passLabel);
            }
        }

        private void LoadClientData()
        {
            _userToEdit = _dbContext.Users
                .FirstOrDefault(u => u.UserID == _clientToEdit.UserID);

            if (_userToEdit == null)
            {
                MessageBox.Show("Ошибка: пользователь клиента не найден в базе данных.\n\n" +
                    "Возможно, данные были повреждены или удалены.\n" +
                    "Обратитесь к администратору системы.",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return;
            }

            LastNameBox.Text = _userToEdit.LastName;
            FirstNameBox.Text = _userToEdit.FirstName;
            LoginBox.Text = _userToEdit.Login;
            LoginBox.IsEnabled = false;

            PassportBox.Text = _clientToEdit.PassportNumber;
            LicenseBox.Text = _clientToEdit.DriverLicenseNumber;
            PhoneBox.Text = _clientToEdit.Phone;
            EmailBox.Text = _clientToEdit.Email ?? string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка ошибок
            LastNameError.Visibility = Visibility.Collapsed;
            FirstNameError.Visibility = Visibility.Collapsed;
            LoginError.Visibility = Visibility.Collapsed;
            PasswordError.Visibility = Visibility.Collapsed;
            PassportError.Visibility = Visibility.Collapsed;
            LicenseError.Visibility = Visibility.Collapsed;
            PhoneError.Visibility = Visibility.Collapsed;

            // Валидация
            bool isValid = true;
            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string passport = PassportBox.Text.Trim();
            string license = LicenseBox.Text.Trim();
            string phone = PhoneBox.Text.Trim();
            string email = EmailBox.Text.Trim();

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

            if (_clientToEdit == null && string.IsNullOrWhiteSpace(password))
            {
                PasswordError.Text = "Пароль обязателен при создании";
                PasswordError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(passport))
            {
                PassportError.Text = "Паспорт обязателен";
                PassportError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(license))
            {
                LicenseError.Text = "Водительское удостоверение обязательно";
                LicenseError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                PhoneError.Text = "Телефон обязателен";
                PhoneError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return;

            try
            {
                if (_clientToEdit == null)
                {
                    // Проверка уникальности логина
                    if (_dbContext.Users.Any(u => u.Login == login))
                    {
                        LoginError.Text = "Логин уже занят другим пользователем";
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
                        RoleID = 3 
                    };

                    _dbContext.Users.Add(newUser);
                    _dbContext.SaveChanges(); // Сохраняем пользователя до создания клиента

                    var newClient = new Client
                    {
                        UserID = newUser.UserID,
                        PassportNumber = passport,
                        DriverLicenseNumber = license,
                        Phone = phone,
                        Email = string.IsNullOrWhiteSpace(email) ? null : email
                    };

                    _dbContext.Clients.Add(newClient);
                }
                else
                {
                    if (_userToEdit == null)
                    {
                        _userToEdit = _dbContext.Users.Find(_clientToEdit.UserID);
                        if (_userToEdit == null)
                        {
                            throw new Exception("Пользователь не найден. Невозможно сохранить изменения.");
                        }
                    }

                    // Редактирование существующего клиента
                    _userToEdit.LastName = lastName;
                    _userToEdit.FirstName = firstName;

                    _clientToEdit.PassportNumber = passport;
                    _clientToEdit.DriverLicenseNumber = license;
                    _clientToEdit.Phone = phone;
                    _clientToEdit.Email = string.IsNullOrWhiteSpace(email) ? null : email;
                }

                _dbContext.SaveChanges();
                MessageBox.Show("Данные успешно сохранены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при сохранении данных:\n{ex.Message}";

                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДетали ошибки:\n{ex.InnerException.Message}";

                    // Дополнительная информация для диагностики
                    if (ex.InnerException.Message.Contains("foreign key"))
                    {
                        errorMessage += "\n\nПричина: Нарушение целостности данных. " +
                            "Проверьте, что все связанные записи существуют в базе данных.";
                    }
                    else if (ex.InnerException.Message.Contains("duplicate key"))
                    {
                        errorMessage += "\n\nПричина: Попытка сохранить дублирующиеся данные. " +
                            "Проверьте уникальность логина и других полей.";
                    }
                }

                MessageBox.Show(errorMessage, "Ошибка сохранения",
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