using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WeeklyShoppingListWPF.Views
{
    public partial class SetBudgetWindow : Window
    {
        public decimal BudgetAmount { get; private set; }

        public SetBudgetWindow(decimal currentBudget = 0)
        {
            InitializeComponent();
            BudgetAmount = currentBudget;
            BudgetTextBox.Text = currentBudget > 0 ? currentBudget.ToString("F0") : string.Empty;
            Loaded += (s, e) =>
            {
                BudgetTextBox.Focus();
                BudgetTextBox.SelectAll();
            };
        }

        private void QuickBudget_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string text)
            {
                string numericOnly = new string(System.Array.FindAll(text.ToCharArray(), char.IsDigit));
                BudgetTextBox.Text = numericOnly;
                BudgetTextBox.Focus();
                BudgetTextBox.CaretIndex = BudgetTextBox.Text.Length;
            }
        }

        private void BudgetTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveButton_Click(sender, e);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string raw = BudgetTextBox.Text?.Trim().Replace(',', '.') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                BudgetAmount = 0;
                DialogResult = true;
                Close();
                return;
            }

            if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val) || val < 0)
            {
                ErrorTextBlock.Text = "Введіть коректну суму (число від 0).";
                ErrorTextBlock.Visibility = Visibility.Visible;
                BudgetTextBox.Focus();
                return;
            }

            BudgetAmount = Math.Round(val, 2);
            DialogResult = true;
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            BudgetAmount = 0;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
