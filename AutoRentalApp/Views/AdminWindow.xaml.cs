using AutoRentalApp.Data;
using AutoRentalApp.Services;
using AutoRentalApp.Views;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class AdminWindow : Window
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _dbContext;

        public AdminWindow(AuthService authService, AppDbContext dbContext)
        {
            InitializeComponent();
            _authService = authService;
            _dbContext = dbContext;

            var user = _authService.GetCurrentUser();
            UserNameText.Text = $"Здравствуйте, {user.FullName}";

            // Загружаем главную страницу по умолчанию
            ShowHomeView();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            new LoginWindow().Show();
            this.Close();
        }

        private void ShowHomeView()
        {
            ContentArea.Content = new TextBlock
            {
                Text = "Добро пожаловать в систему управления прокатом автомобилей!\n\n" +
                       "Выберите раздел в меню слева для начала работы:\n" +
                       "• Клиенты — управление клиентской базой\n" +
                       "• Автомобили — управление автопарком\n" +
                       "• Сотрудники — управление персоналом\n" +
                       "• Договоры — управление договорами аренды",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHomeView();
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ClientsView(_dbContext);
        }

        private void CarsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new CarsView(_dbContext);
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new EmployeesView(_dbContext);
        }

       /* private void ContractsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ContractsView(_dbContext);
        }*/
    }
}