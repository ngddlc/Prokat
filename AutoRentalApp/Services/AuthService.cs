using AutoRentalApp.Data;
using AutoRentalApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AutoRentalApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private User _currentUser;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Хэширование пароля 
        /// </summary>
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public (bool success, string message) Register(
     string firstName,
     string lastName,
     string login,
     string password,
     string confirmPassword,
     int roleId)
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(firstName))
                return (false, "Имя не может быть пустым");

            if (string.IsNullOrWhiteSpace(lastName))
                return (false, "Фамилия не может быть пустой");

            if (string.IsNullOrWhiteSpace(login))
                return (false, "Логин не может быть пустым");

            if (login.Length < 3)
                return (false, "Логин должен содержать минимум 3 символа");

            if (string.IsNullOrWhiteSpace(password))
                return (false, "Пароль не может быть пустым");

            if (password.Length < 6)
                return (false, "Пароль должен содержать минимум 6 символов");

            if (password != confirmPassword)
                return (false, "Пароли не совпадают");

            if (_context.Users.Any(u => u.Login == login))
                return (false, "Пользователь с таким логином уже существует");

            string passwordHash = HashPassword(password);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Login = login,
                PasswordHash = passwordHash,
                RoleID = roleId
            };

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges(); // Сохраняем пользователя до создания зависимых записей

                if (roleId == 3) 
                {
                    var client = new Client
                    {
                        UserID = user.UserID,
                        PassportNumber = "",
                        DriverLicenseNumber = "",
                        Phone = "",
                        Email = null
                    };
                    _context.Clients.Add(client);
                }
                else if (roleId == 2) 
                {
                    var manager = new Employee
                    {
                        UserID = user.UserID,
                        Position = "менеджер",
                        HireDate = DateTime.Today
                    };
                    _context.Employees.Add(manager);
                }
                // Администраторы не создают записей в employees 

                _context.SaveChanges();
                return (true, "Регистрация успешна!");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        public (bool success, string message, User user) Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return (false, "Логин и пароль обязательны", null);

            string passwordHash = HashPassword(password);

            // Поиск пользователя
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == passwordHash);

            if (user == null)
                return (false, "Неверный логин или пароль", null);

            _currentUser = user;
            return (true, "Авторизация успешна", user);
        }

        /// <summary>
        /// Получение текущего пользователя
        /// </summary>
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
        }

        /// <summary>
        /// Проверка, авторизован ли пользователь
        /// </summary>
        public bool IsAuthenticated()
        {
            return _currentUser != null;
        }
    }
}