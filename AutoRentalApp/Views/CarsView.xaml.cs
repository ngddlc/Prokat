using AutoRentalApp.Data;
using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class CarsView : UserControl
    {
        private readonly AppDbContext _dbContext;
        private System.Collections.Generic.List<Car> _allCars;

        public CarsView(AppDbContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                StatusText.Text = "Загрузка данных...";

                _allCars = _dbContext.Cars
                    .Include(c => c.CarStatus)
                    .ToList();

                CarsDataGrid.ItemsSource = _allCars;
                StatusText.Text = $"Всего автомобилей: {_allCars.Count}";
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
            FilterCars();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterCars();
        }

        private void FilterCars()
        {
            string query = SearchBox.Text?.ToLower().Trim() ?? "";

            if (string.IsNullOrWhiteSpace(query))
            {
                CarsDataGrid.ItemsSource = _allCars;
                StatusText.Text = $"Все автомобили: {_allCars.Count}";
                return;
            }

            var filtered = _allCars.Where(c =>
                c.Brand.ToLower().Contains(query) ||
                c.Model.ToLower().Contains(query) ||
                c.PlateNumber.ToLower().Contains(query) ||
                c.Color.ToLower().Contains(query)
            ).ToList();

            CarsDataGrid.ItemsSource = filtered;
            StatusText.Text = filtered.Any()
                ? $"Найдено: {filtered.Count} автомобилей"
                : "Автомобили не найдены";
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditCarWindow(_dbContext, null);
            if (addWindow.ShowDialog() == true)
            {
                LoadCars();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            LoadCars();
        }

        private void CarsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Действия в строке деталей
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var car = button?.Tag as Car;

            if (car != null)
            {
                var editWindow = new AddEditCarWindow(_dbContext, car);
                if (editWindow.ShowDialog() == true)
                {
                    LoadCars();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var car = button?.Tag as Car;

            if (car == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить автомобиль {car.Brand} {car.Model} ({car.PlateNumber})?\n\n" +
                "⚠️ ВНИМАНИЕ: Будут автоматически удалены:\n" +
                "• Все договоры аренды этого автомобиля\n" +
                "• Все осмотры автомобилей по этим договорам",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Получаем все договоры автомобиля
                    var contracts = _dbContext.RentalContracts
                        .Where(rc => rc.CarID == car.CarID)
                        .ToList();

                    // Собираем все осмотры по этим договорам
                    var inspectionIds = contracts.Select(c => c.ContractID).ToList();
                    var inspections = _dbContext.CarInspections
                        .Where(ci => inspectionIds.Contains(ci.ContractID))
                        .ToList();

                    // Удаляем в правильном порядке
                    _dbContext.CarInspections.RemoveRange(inspections);
                    _dbContext.RentalContracts.RemoveRange(contracts);
                    _dbContext.Cars.Remove(car);
                    _dbContext.SaveChanges();

                    LoadCars();
                    MessageBox.Show($"Автомобиль и все связанные данные успешно удалены.\n" +
                        $"Удалено договоров: {contracts.Count}\n" +
                        $"Удалено осмотров: {inspections.Count}",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string errorMsg = "Ошибка при удалении автомобиля:\n" + ex.Message;

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
