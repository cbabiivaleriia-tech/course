using System;

namespace WeeklyShoppingListWPF.Models
{
    /// <summary>
    /// Модель користувача системи.
    /// </summary>
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Логін користувача (унікальний).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Повне ім'я або відображуваний нікнейм.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Хеш пароля (PBKDF2-SHA256 у форматі Base64).
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Криптографічна сіль (унікальна для кожного користувача).
        /// </summary>
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// Встановлений бюджет користувача на покупки.
        /// </summary>
        public decimal BudgetLimit { get; set; } = 0;

        /// <summary>
        /// Дата реєстрації облікового запису.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
