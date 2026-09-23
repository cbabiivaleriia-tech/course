using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using WeeklyShoppingListWPF.Models;

namespace WeeklyShoppingListWPF.Services
{
    /// <summary>
    /// Сервіс для збереження, завантаження та переміщення покупок і архіву у форматі JSON.
    /// Підтримує роздільне збереження даних для кожного користувача.
    /// </summary>
    public class JsonStorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        private const string DefaultListFileName = "shopping_list.json";
        private const string DefaultArchiveFileName = "archive.json";

        /// <summary>
        /// Повертає шлях до файлу списку покупок для вказаного користувача (або загального).
        /// </summary>
        public static string GetListFilePath(string? username = null)
        {
            string user = string.IsNullOrWhiteSpace(username) ? (AuthService.CurrentUser?.Username ?? string.Empty) : username;
            string fileName = string.IsNullOrWhiteSpace(user) 
                ? DefaultListFileName 
                : $"shopping_list_{SanitizeFileName(user)}.json";

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        /// <summary>
        /// Повертає шлях до файлу архіву куплених товарів для вказаного користувача (або загального).
        /// </summary>
        public static string GetArchiveFilePath(string? username = null)
        {
            string user = string.IsNullOrWhiteSpace(username) ? (AuthService.CurrentUser?.Username ?? string.Empty) : username;
            string fileName = string.IsNullOrWhiteSpace(user) 
                ? DefaultArchiveFileName 
                : $"archive_{SanitizeFileName(user)}.json";

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.ToLowerInvariant();
        }

        /// <summary>
        /// Серіалізація списку покупок у рядок JSON.
        /// </summary>
        public string SerializeToJson(IEnumerable<ShoppingItem> items)
        {
            if (items == null) return "[]";
            return JsonSerializer.Serialize(items, JsonOptions);
        }

        /// <summary>
        /// Парсинг рядка JSON у список покупок.
        /// </summary>
        public List<ShoppingItem> DeserializeFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<ShoppingItem>();

            try
            {
                return JsonSerializer.Deserialize<List<ShoppingItem>>(json, JsonOptions) ?? new List<ShoppingItem>();
            }
            catch
            {
                return new List<ShoppingItem>();
            }
        }

        /// <summary>
        /// Запис списку покупок у JSON-файл за вказаним шляхом.
        /// </summary>
        public void SaveToFile(string filePath, IEnumerable<ShoppingItem> items)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(filePath));
            }

            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = SerializeToJson(items);
            File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Завантаження списку покупок із JSON-файлу.
        /// </summary>
        public List<ShoppingItem> LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return new List<ShoppingItem>();
            }

            try
            {
                string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                return DeserializeFromJson(json);
            }
            catch
            {
                return new List<ShoppingItem>();
            }
        }

        // ============================================
        // ОПЕРАЦІЇ ЗІ СПИСКОМ ПОКУПОК
        // ============================================

        public List<ShoppingItem> LoadShoppingList(string? username = null)
        {
            return LoadFromFile(GetListFilePath(username));
        }

        public void SaveShoppingList(IEnumerable<ShoppingItem> items, string? username = null)
        {
            SaveToFile(GetListFilePath(username), items);
        }

        // Збереження сумісності зі старим кодом
        public List<ShoppingItem> LoadDefault() => LoadShoppingList();
        public void SaveDefault(IEnumerable<ShoppingItem> items) => SaveShoppingList(items);

        // ============================================
        // ОПЕРАЦІЇ З АРХІВОМ ПОКУПОК
        // ============================================

        public List<ShoppingItem> LoadArchive(string? username = null)
        {
            return LoadFromFile(GetArchiveFilePath(username));
        }

        public void SaveArchive(IEnumerable<ShoppingItem> items, string? username = null)
        {
            SaveToFile(GetArchiveFilePath(username), items);
        }

        /// <summary>
        /// Переміщення всіх куплених (IsPurchased == true) товарів зі списку до архіву.
        /// </summary>
        public int ArchivePurchasedItems(ICollection<ShoppingItem> activeItems, string? username = null)
        {
            var purchased = activeItems.Where(i => i.IsPurchased).ToList();
            if (purchased.Count == 0) return 0;

            var archive = LoadArchive(username);

            foreach (var item in purchased)
            {
                item.ArchivedAt = DateTime.Now;
                archive.Insert(0, item); // додаємо на початок архіву
                activeItems.Remove(item);
            }

            SaveShoppingList(activeItems, username);
            SaveArchive(archive, username);

            return purchased.Count;
        }

        /// <summary>
        /// Відновлення товару з архіву назад у список активних покупок.
        /// </summary>
        public void RestoreItemFromArchive(ShoppingItem item, string? username = null)
        {
            var archive = LoadArchive(username);
            var active = LoadShoppingList(username);

            var existingInArchive = archive.FirstOrDefault(i => i.Id == item.Id);
            if (existingInArchive != null)
            {
                archive.Remove(existingInArchive);
            }

            item.IsPurchased = false;
            item.ArchivedAt = null;
            active.Add(item);

            SaveArchive(archive, username);
            SaveShoppingList(active, username);
        }

        /// <summary>
        /// Остаточне видалення товару з архіву.
        /// </summary>
        public void DeleteFromArchive(string itemId, string? username = null)
        {
            var archive = LoadArchive(username);
            var item = archive.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                archive.Remove(item);
                SaveArchive(archive, username);
            }
        }

        /// <summary>
        /// Повне очищення архіву.
        /// </summary>
        public void ClearArchive(string? username = null)
        {
            SaveArchive(new List<ShoppingItem>(), username);
        }
    }
}
