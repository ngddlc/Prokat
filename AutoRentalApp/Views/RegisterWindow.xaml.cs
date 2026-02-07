using AutoRentalApp.Data;
using AutoRentalApp.Helpers;
using AutoRentalApp.Services;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService;

        public RegisterWindow()
        {
            InitializeComponent();

            // Инициализация сервиса авторизации
            string connectionString = DbHelper.GetConnectionString();
            var dbContext = new AppDbContext(connectionString);
            _authService = new AuthService(dbContext);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка ошибок
            LoginErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
            ConfirmPasswordErrorText.Visibility = Visibility.Collapsed;

            // Получение данных из формы
            string lastName = LastNameBox.Text.Trim();
            string firstName = FirstNameBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            // Получение выбранной роли
            var selectedRole = RoleComboBox.SelectedItem as ComboBoxItem;
            int roleId = selectedRole != null ? int.Parse(selectedRole.Tag.ToString()) : 3;

            // Валидация на клиенте
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Введите имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(login))
            {
                LoginErrorText.Text = "Логин обязателен";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (login.Length < 3)
            {
                LoginErrorText.Text = "Логин должен быть минимум 3 символа";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordErrorText.Text = "Пароль обязателен";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (password.Length < 6)
            {
                PasswordErrorText.Text = "Пароль должен быть минимум 6 символов";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (password != confirmPassword)
            {
                ConfirmPasswordErrorText.Text = "Пароли не совпадают";
                ConfirmPasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return;

            // Регистрация через сервис
            var (success, message) = _authService.Register(
                firstName, lastName, login, password, confirmPassword, roleId);

            if (success)
            {
                MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(message, "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}