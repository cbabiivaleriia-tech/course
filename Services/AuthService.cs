using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WeeklyShoppingListWPF.Models;

namespace WeeklyShoppingListWPF.Services
{
    /// <summary>
    /// Сервіс авторизації, реєстрації та керування сесією користувача з хешуванням паролів (PBKDF2-SHA256).
    /// </summary>
    public class AuthService
    {
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int Pbkdf2Iterations = 100_000;
        private const string UsersFileName = "users.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Поточний авторизований користувач у додатку.
        /// </summary>
        public static User? CurrentUser { get; private set; }

        /// <summary>
        /// Подія при зміні користувача або оновленні його профілю.
        /// </summary>
        public static event Action? CurrentUserChanged;

        private static string GetUsersFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, UsersFileName);
        }

        /// <summary>
        /// Завантаження списку всіх користувачів із файлу.
        /// </summary>
        public List<User> LoadUsers()
        {
            string path = GetUsersFilePath();
            if (!File.Exists(path))
            {
                return new List<User>();
            }

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<User>>(json, JsonOptions) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        /// <summary>
        /// Збереження списку користувачів у файл.
        /// </summary>
        public void SaveUsers(IEnumerable<User> users)
        {
            string path = GetUsersFilePath();
            string json = JsonSerializer.Serialize(users, JsonOptions);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        /// <summary>
        /// Реєстрація нового користувача з криптографічним хешуванням пароля.
        /// </summary>
        public (bool success, string message) Register(string username, string fullName, string password, string confirmPassword)
        {
            username = username?.Trim() ?? string.Empty;
            fullName = fullName?.Trim() ?? string.Empty;
            password = password ?? string.Empty;
            confirmPassword = confirmPassword ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                return (false, "Логін не може бути порожнім.");
            }

            if (username.Length < 3)
            {
                return (false, "Логін повинен містити щонайменше 3 символи.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Пароль не може бути порожнім.");
            }

            if (password.Length < 4)
            {
                return (false, "Пароль повинен бути довжиною від 4 символів.");
            }

            if (password != confirmPassword)
            {
                return (false, "Паролі не збігаються.");
            }

            var users = LoadUsers();
            if (users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Користувач із таким логіном вже існує.");
            }

            // Генерація криптографічної солі
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            string salt = Convert.ToBase64String(saltBytes);

            // Обчислення безпечного хешу PBKDF2-SHA256
            string hash = HashPassword(password, saltBytes);

            var newUser = new User
            {
                Username = username,
                FullName = string.IsNullOrWhiteSpace(fullName) ? username : fullName,
                PasswordHash = hash,
                Salt = salt,
                BudgetLimit = 0,
                CreatedAt = DateTime.Now
            };

            users.Add(newUser);
            SaveUsers(users);

            SetCurrentUser(newUser);
            return (true, "Реєстрація успішна!");
        }

        /// <summary>
        /// Авторизація користувача за логіном та паролем.
        /// </summary>
        public (bool success, string message) Login(string username, string password)
        {
            username = username?.Trim() ?? string.Empty;
            password = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Будь ласка, введіть логін та пароль.");
            }

            var users = LoadUsers();
            var user = users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return (false, "Користувача з таким логіном не знайдено.");
            }

            try
            {
                byte[] saltBytes = Convert.FromBase64String(user.Salt);
                byte[] expectedHash = Convert.FromBase64String(user.PasswordHash);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    saltBytes,
                    Pbkdf2Iterations,
                    HashAlgorithmName.SHA256,
                    HashSizeBytes);

                // Безпечне порівняння константного часу для запобігання атакам по часу (Timing Attacks)
                if (CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
                {
                    SetCurrentUser(user);
                    return (true, "Авторизація успішна!");
                }
            }
            catch
            {
                return (false, "Помилка перевірки пароля.");
            }

            return (false, "Невірний пароль.");
        }

        /// <summary>
        /// Оновлення бюджету поточного користувача.
        /// </summary>
        public void UpdateCurrentBudget(decimal newBudget)
        {
            if (CurrentUser == null) return;

            CurrentUser.BudgetLimit = Math.Max(0, newBudget);

            var users = LoadUsers();
            var existing = users.FirstOrDefault(u => u.Id == CurrentUser.Id);
            if (existing != null)
            {
                existing.BudgetLimit = CurrentUser.BudgetLimit;
                SaveUsers(users);
            }

            CurrentUserChanged?.Invoke();
        }

        /// <summary>
        /// Встановлення активного користувача та сповіщення підписників.
        /// </summary>
        public void SetCurrentUser(User? user)
        {
            CurrentUser = user;
            CurrentUserChanged?.Invoke();
        }

        /// <summary>
        /// Вихід із системи (Logout).
        /// </summary>
        public void Logout()
        {
            SetCurrentUser(null);
        }

        /// <summary>
        /// Обчислення хешу пароля за допомогою PBKDF2 з SHA-256.
        /// </summary>
        private static string HashPassword(string password, byte[] saltBytes)
        {
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                Pbkdf2Iterations,
                HashAlgorithmName.SHA256,
                HashSizeBytes);

            return Convert.ToBase64String(hash);
        }
    }
}
