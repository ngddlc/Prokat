using AutoRentalApp.Data;
using AutoRentalApp.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoRentalApp.Views
{
    public partial class AddEditCarWindow : Window
    {
        private readonly AppDbContext _dbContext;
        private readonly Car _carToEdit;

        public AddEditCarWindow(AppDbContext dbContext, Car car)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _carToEdit = car;

            // Загрузка статусов в выпадающий список
            var statuses = _dbContext.CarStatuses.ToList();
            foreach (var status in statuses)
            {
                StatusComboBox.Items.Add(new ComboBoxItem { Content = status.StatusName, Tag = status.CarStatusID });
            }

            if (car != null)
            {
                Title = "Редактирование автомобиля";
                TitleText.Text = "Редактирование автомобиля";
                LoadCarData();
            }
        }

        private void LoadCarData()
        {
            PlateNumberBox.Text = _carToEdit.PlateNumber;
            VINBox.Text = _carToEdit.VIN;
            BrandBox.Text = _carToEdit.Brand;
            ModelBox.Text = _carToEdit.Model;
            YearBox.Text = _carToEdit.Year.ToString();
            ColorBox.Text = _carToEdit.Color;
            BodyTypeBox.Text = _carToEdit.BodyType;
            EngineTypeBox.Text = _carToEdit.EngineType;
            EngineVolumeBox.Text = _carToEdit.EngineVolume.ToString();
            TransmissionBox.Text = _carToEdit.TransmissionType;
            ClassComboBox.Text = _carToEdit.CarClass;
            DailyPriceBox.Text = _carToEdit.DailyPrice.ToString();
            MileageBox.Text = _carToEdit.Mileage.ToString();

            // Выбор статуса
            var statusItem = StatusComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => (int)item.Tag == _carToEdit.CarStatusID);
            if (statusItem != null)
                StatusComboBox.SelectedItem = statusItem;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Очистка ошибок
            PlateNumberError.Visibility = Visibility.Collapsed;
            VINError.Visibility = Visibility.Collapsed;
            BrandError.Visibility = Visibility.Collapsed;
            ModelError.Visibility = Visibility.Collapsed;
            YearError.Visibility = Visibility.Collapsed;
            ColorError.Visibility = Visibility.Collapsed;
            BodyTypeError.Visibility = Visibility.Collapsed;
            EngineTypeError.Visibility = Visibility.Collapsed;
            EngineVolumeError.Visibility = Visibility.Collapsed;
            TransmissionError.Visibility = Visibility.Collapsed;
            DailyPriceError.Visibility = Visibility.Collapsed;
            MileageError.Visibility = Visibility.Collapsed;

            // Валидация
            bool isValid = true;
            string plateNumber = PlateNumberBox.Text.Trim();
            string vin = VINBox.Text.Trim();
            string brand = BrandBox.Text.Trim();
            string model = ModelBox.Text.Trim();
            string yearText = YearBox.Text.Trim();
            string color = ColorBox.Text.Trim();
            string bodyType = BodyTypeBox.Text.Trim();
            string engineType = EngineTypeBox.Text.Trim();
            string engineVolumeText = EngineVolumeBox.Text.Trim();
            string transmission = TransmissionBox.Text.Trim();
            string carClass = ClassComboBox.Text.Trim();
            string dailyPriceText = DailyPriceBox.Text.Trim();
            string mileageText = MileageBox.Text.Trim();
            var selectedStatus = StatusComboBox.SelectedItem as ComboBoxItem;

            if (string.IsNullOrWhiteSpace(plateNumber))
            {
                PlateNumberError.Text = "Гос. номер обязателен";
                PlateNumberError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(vin) || vin.Length != 17)
            {
                VINError.Text = "VIN должен содержать 17 символов";
                VINError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(brand))
            {
                BrandError.Text = "Марка обязательна";
                BrandError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                ModelError.Text = "Модель обязательна";
                ModelError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(yearText, out int year) || year < 1990 || year > 2030)
            {
                YearError.Text = "Год должен быть от 1990 до 2030";
                YearError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(color))
            {
                ColorError.Text = "Цвет обязателен";
                ColorError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(bodyType))
            {
                BodyTypeError.Text = "Тип кузова обязателен";
                BodyTypeError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(engineType))
            {
                EngineTypeError.Text = "Тип двигателя обязателен";
                EngineTypeError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!decimal.TryParse(engineVolumeText, out decimal engineVolume) || engineVolume <= 0)
            {
                EngineVolumeError.Text = "Объём двигателя должен быть положительным числом";
                EngineVolumeError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(transmission))
            {
                TransmissionError.Text = "Тип КПП обязателен";
                TransmissionError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(carClass))
            {
                // Уже выбран по умолчанию
            }

            if (!decimal.TryParse(dailyPriceText, out decimal dailyPrice) || dailyPrice <= 0)
            {
                DailyPriceError.Text = "Цена должна быть положительным числом";
                DailyPriceError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!int.TryParse(mileageText, out int mileage) || mileage < 0)
            {
                MileageError.Text = "Пробег должен быть неотрицательным числом";
                MileageError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (selectedStatus == null)
            {
                MessageBox.Show("Выберите статус автомобиля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                isValid = false;
            }

            if (!isValid)
                return;

            try
            {
                if (_carToEdit == null)
                {
                    // Проверка уникальности гос. номера и VIN
                    if (_dbContext.Cars.Any(c => c.PlateNumber == plateNumber))
                    {
                        PlateNumberError.Text = "Автомобиль с таким гос. номером уже существует";
                        PlateNumberError.Visibility = Visibility.Visible;
                        return;
                    }

                    if (_dbContext.Cars.Any(c => c.VIN == vin))
                    {
                        VINError.Text = "Автомобиль с таким VIN уже существует";
                        VINError.Visibility = Visibility.Visible;
                        return;
                    }

                    var newCar = new Car
                    {
                        PlateNumber = plateNumber,
                        VIN = vin,
                        Brand = brand,
                        Model = model,
                        Year = year,
                        Color = color,
                        BodyType = bodyType,
                        EngineType = engineType,
                        EngineVolume = engineVolume,
                        TransmissionType = transmission,
                        CarClass = carClass,
                        DailyPrice = dailyPrice,
                        Mileage = mileage,
                        CarStatusID = (int)selectedStatus.Tag
                    };

                    _dbContext.Cars.Add(newCar);
                }
                else
                {
                    // Редактирование существующего автомобиля
                    _carToEdit.PlateNumber = plateNumber;
                    _carToEdit.VIN = vin;
                    _carToEdit.Brand = brand;
                    _carToEdit.Model = model;
                    _carToEdit.Year = year;
                    _carToEdit.Color = color;
                    _carToEdit.BodyType = bodyType;
                    _carToEdit.EngineType = engineType;
                    _carToEdit.EngineVolume = engineVolume;
                    _carToEdit.TransmissionType = transmission;
                    _carToEdit.CarClass = carClass;
                    _carToEdit.DailyPrice = dailyPrice;
                    _carToEdit.Mileage = mileage;
                    _carToEdit.CarStatusID = (int)selectedStatus.Tag;
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}