using AutoRentalApp.Data;
using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class ClientCarsCatalog : Window
    {
        private readonly AppDbContext _dbContext;
        private System.Collections.Generic.List<Car> _allCars;
        private Client _currentClient;

        public ClientCarsCatalog(AppDbContext dbContext, Client currentClient)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _currentClient = currentClient;
            LoadCars();
        }

        private void LoadCars()
        {
            try
            {
                StatusText.Text = "Загрузка данных...";

                _allCars = _dbContext.Cars
                    .Include(c => c.CarStatus)
                    .Where(c => c.CarStatusID == 1) // Только свободные автомобили
                    .ToList();

                CarsDataGrid.ItemsSource = _allCars;
                StatusText.Text = $"Доступно автомобилей: {_allCars.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Ошибка загрузки";
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ClassFilterComboBox.SelectedIndex = 0;
            PriceFromBox.Clear();
            PriceToBox.Clear();
            CarsDataGrid.ItemsSource = _allCars;
            StatusText.Text = $"Все автомобили: {_allCars.Count}";
        }

        private void ApplyFilters()
        {
            var filtered = _allCars.AsQueryable();

            // Фильтр по классу
            var selectedClass = ClassFilterComboBox.SelectedItem as ComboBoxItem;
            if (selectedClass != null && selectedClass.Content.ToString() != "Все")
            {
                filtered = filtered.Where(c => c.CarClass == selectedClass.Content.ToString());
            }

            // Фильтр по цене от
            if (!string.IsNullOrWhiteSpace(PriceFromBox.Text) && decimal.TryParse(PriceFromBox.Text, out decimal priceFrom))
            {
                filtered = filtered.Where(c => c.DailyPrice >= priceFrom);
            }

            // Фильтр по цене до
            if (!string.IsNullOrWhiteSpace(PriceToBox.Text) && decimal.TryParse(PriceToBox.Text, out decimal priceTo))
            {
                filtered = filtered.Where(c => c.DailyPrice <= priceTo);
            }

            var result = filtered.ToList();
            CarsDataGrid.ItemsSource = result;
            StatusText.Text = result.Any()
                ? $"Найдено: {result.Count} автомобилей"
                : "Автомобили не найдены";
        }

        private void CarsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void RentCarButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var car = button?.Tag as Car;

            if (car != null)
            {
                // Открываем форму оформления аренды
                var rentWindow = new ClientRentCarWindow(_dbContext, _currentClient, car);
                rentWindow.ShowDialog();

                // Обновляем список после аренды
                LoadCars();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}