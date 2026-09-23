using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using WeeklyShoppingListWPF.Models;

namespace WeeklyShoppingListWPF.Views
{
    public partial class AddPurchaseWindow : Window
    {
        public ShoppingItem? CreatedItem { get; private set; }

        public AddPurchaseWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => NameTextBox.Focus();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError("Будь ласка, введіть назву товару.");
                NameTextBox.Focus();
                return;
            }

            string qtyText = QuantityTextBox.Text?.Trim().Replace(',', '.') ?? "1";
            if (!double.TryParse(qtyText, NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity) || quantity <= 0)
            {
                ShowError("Введіть коректну кількість (більше 0).");
                QuantityTextBox.Focus();
                return;
            }

            string priceText = PriceTextBox.Text?.Trim().Replace(',', '.') ?? "0";
            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price < 0)
            {
                ShowError("Введіть коректну ціну (0 або більше).");
                PriceTextBox.Focus();
                return;
            }

            string unit = (UnitComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "шт";
            string category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Продукти";

            CreatedItem = new ShoppingItem
            {
                Name = name,
                Quantity = quantity,
                Unit = unit,
                Price = price,
                Category = category,
                IsPurchased = false,
                CreatedAt = DateTime.Now
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}
