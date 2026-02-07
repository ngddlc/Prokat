using AutoRentalApp.Data;
using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class ClientsView : UserControl
    {
        private readonly AppDbContext _dbContext;
        private System.Collections.Generic.List<Client> _allClients;

        public ClientsView(AppDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                StatusText.Text = "Загрузка данных...";

                _allClients = _dbContext.Clients
                    .Include(c => c.User)
                    .ToList();

                ClientsDataGrid.ItemsSource = _allClients;
                StatusText.Text = $"Всего клиентов: {_allClients.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки";
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClients();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterClients();
        }

        private void FilterClients()
        {
            string query = SearchBox.Text?.ToLower().Trim() ?? "";

            if (string.IsNullOrWhiteSpace(query))
            {
                ClientsDataGrid.ItemsSource = _allClients;
                StatusText.Text = $"Все клиенты: {_allClients.Count}";
                return;
            }

            var filtered = _allClients.Where(c =>
                c.User.FirstName.ToLower().Contains(query) ||
                c.User.LastName.ToLower().Contains(query) ||
                c.Phone.ToLower().Contains(query) ||
                (c.Email != null && c.Email.ToLower().Contains(query))
            ).ToList();

            ClientsDataGrid.ItemsSource = filtered;
            StatusText.Text = filtered.Any()
                ? $"Найдено: {filtered.Count} клиентов"
                : "Клиенты не найдены";
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditClientWindow(_dbContext, null);
            if (addWindow.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            LoadClients();
        }

        private void ClientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Действия в строке деталей (кнопки Редактировать/Удалить)
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var client = button?.Tag as Client;

            if (client != null)
            {
                var editWindow = new AddEditClientWindow(_dbContext, client);
                if (editWindow.ShowDialog() == true)
                {
                    LoadClients();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var client = button?.Tag as Client;

            if (client == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить клиента {client.User.FullName}?\n\n" +
                "⚠️ ВНИМАНИЕ: Будут автоматически удалены:\n" +
                "• Все договоры аренды этого клиента\n" +
                "• Все осмотры автомобилей по этим договорам\n" +
                "• Запись пользователя из системы",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Получаем всех договоров клиента
                    var contracts = _dbContext.RentalContracts
                        .Where(rc => rc.ClientID == client.ClientID)
                        .ToList();

                    // Собираем все осмотры по этим договорам
                    var inspectionIds = contracts.Select(c => c.ContractID).ToList();
                    var inspections = _dbContext.CarInspections
                        .Where(ci => inspectionIds.Contains(ci.ContractID))
                        .ToList();

                    // Удаляем в правильном порядке: сначала осмотры, потом договоры
                    _dbContext.CarInspections.RemoveRange(inspections);
                    _dbContext.RentalContracts.RemoveRange(contracts);
                    _dbContext.Clients.Remove(client);

                    // ИСПРАВЛЕНО: Удаляем запись пользователя из таблицы users
                    var user = _dbContext.Users.Find(client.UserID);
                    if (user != null)
                    {
                        _dbContext.Users.Remove(user);
                    }

                    _dbContext.SaveChanges();

                    LoadClients();
                    MessageBox.Show($"Клиент и все связанные данные успешно удалены.\n" +
                        $"Удалено договоров: {contracts.Count}\n" +
                        $"Удалено осмотров: {inspections.Count}\n" +
                        $"Удалена запись пользователя из системы",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string errorMsg = "Ошибка при удалении клиента:\n" + ex.Message;

                    if (ex.InnerException != null && ex.InnerException.Message.Contains("foreign key"))
                    {
                        errorMsg += "\n\nПричина: Нарушение целостности данных. " +
                            "Возможно, есть другие зависимости, которые нужно удалить вручную через pgAdmin.";
                    }

                    MessageBox.Show(errorMsg, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    }
