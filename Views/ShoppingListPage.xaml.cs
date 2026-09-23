using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeeklyShoppingListWPF.Models;
using WeeklyShoppingListWPF.Services;

namespace WeeklyShoppingListWPF.Views
{
    public partial class ShoppingListPage : Page
    {
        private readonly JsonStorageService _jsonService = new JsonStorageService();
        private readonly AuthService _authService = new AuthService();

        public ObservableCollection<ShoppingItem> Items { get; } = new ObservableCollection<ShoppingItem>();

        public ShoppingListPage()
        {
            InitializeComponent();
            ShoppingItemsControl.ItemsSource = Items;
            AuthService.CurrentUserChanged += OnCurrentUserChanged;
        }

        private void OnCurrentUserChanged()
        {
            Dispatcher.Invoke(() =>
            {
                LoadItems();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                var loaded = _jsonService.LoadShoppingList();
                Items.Clear();

                foreach (var item in loaded)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.PropertyChanged += Item_PropertyChanged;
                    Items.Add(item);
                }
            }
            catch
            {
                // Помилка читання
            }

            UpdateSummary();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _jsonService.SaveShoppingList(Items);
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            decimal totalPrice = Items.Sum(item => item.TotalPrice);
            int totalCount = Items.Count;
            int purchasedCount = Items.Count(item => item.IsPurchased);

            TotalPriceTextBlock.Text = $"{totalPrice:N2} грн";
            ItemsCountSubTextBlock.Text = $"Всього позицій: {totalCount}";

            int percentagePurchased = totalCount > 0 ? (int)Math.Round((double)purchasedCount / totalCount * 100) : 0;
            PurchasedCountTextBlock.Text = $"{purchasedCount} з {totalCount}";
            ReadyToArchiveTextBlock.Text = $"Куплено: {percentagePurchased}%";

            ArchivePurchasedButton.Content = $"📦  В архів ({purchasedCount})";
            ArchivePurchasedButton.IsEnabled = purchasedCount > 0;

            // Логіка бюджету
            decimal budget = AuthService.CurrentUser?.BudgetLimit ?? 0;

            if (budget > 0)
            {
                BudgetTextBlock.Text = $"{budget:N2} грн";
                decimal remaining = budget - totalPrice;

                double spentPercent = (double)(totalPrice / budget * 100.0m);
                BudgetProgressBar.Value = Math.Min(100, Math.Max(0, spentPercent));

                if (remaining >= 0)
                {
                    BudgetRemainingTextBlock.Text = $"{remaining:N2} грн";
                    BudgetRemainingTextBlock.Foreground = (Brush)FindResource("SuccessBrush");
                    BudgetRemainingStatusTextBlock.Text = "В межах ліміту";
                    BudgetRemainingStatusTextBlock.Foreground = (Brush)FindResource("SuccessBrush");

                    if (spentPercent >= 80)
                    {
                        BudgetProgressBar.Foreground = (Brush)FindResource("WarningBrush");
                        BudgetHintTextBlock.Text = $"Увага: використано {spentPercent:F0}% бюджету. Залишилося {remaining:N2} грн.";
                    }
                    else
                    {
                        BudgetProgressBar.Foreground = (Brush)FindResource("PrimaryBrush");
                        BudgetHintTextBlock.Text = $"Використано {spentPercent:F0}% бюджету. До ліміту залишилося {remaining:N2} грн.";
                    }
                }
                else
                {
                    decimal overBudget = Math.Abs(remaining);
                    BudgetRemainingTextBlock.Text = $"-{overBudget:N2} грн";
                    BudgetRemainingTextBlock.Foreground = (Brush)FindResource("DangerBrush");
                    BudgetRemainingStatusTextBlock.Text = "⚠️ Ліміт перевищено!";
                    BudgetRemainingStatusTextBlock.Foreground = (Brush)FindResource("DangerBrush");

                    BudgetProgressBar.Foreground = (Brush)FindResource("DangerBrush");
                    BudgetProgressBar.Value = 100;
                    BudgetHintTextBlock.Text = $"Увага! Бюджет перевищено на {overBudget:N2} грн! Збільшіть ліміт або перегляньте товари.";
                }
            }
            else
            {
                BudgetTextBlock.Text = "Не встановлено";
                BudgetRemainingTextBlock.Text = "—";
                BudgetRemainingTextBlock.Foreground = (Brush)FindResource("SecondaryTextBrush");
                BudgetRemainingStatusTextBlock.Text = "Без обмеження";
                BudgetRemainingStatusTextBlock.Foreground = (Brush)FindResource("SecondaryTextBrush");

                BudgetProgressBar.Value = 0;
                BudgetHintTextBlock.Text = "Встановіть ліміт бюджету (натисніть «✏️ Змінити»), щоб контролювати витрати.";
            }

            // Відображення порожнього списку
            if (totalCount == 0)
            {
                EmptyStateBorder.Visibility = Visibility.Visible;
                ItemsScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyStateBorder.Visibility = Visibility.Collapsed;
                ItemsScrollViewer.Visibility = Visibility.Visible;
            }
        }

        private void EditBudget_Click(object sender, RoutedEventArgs e)
        {
            decimal currentBudget = AuthService.CurrentUser?.BudgetLimit ?? 0;
            var dialog = new SetBudgetWindow(currentBudget)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                _authService.UpdateCurrentBudget(dialog.BudgetAmount);
                UpdateSummary();
            }
        }

        private void AddPurchase_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddPurchaseWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.CreatedItem != null)
            {
                dialog.CreatedItem.PropertyChanged += Item_PropertyChanged;
                Items.Add(dialog.CreatedItem);
                _jsonService.SaveShoppingList(Items);
                UpdateSummary();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ShoppingItem item })
            {
                item.PropertyChanged -= Item_PropertyChanged;
                Items.Remove(item);
                _jsonService.SaveShoppingList(Items);
                UpdateSummary();
            }
        }

        private void ItemPurchased_Click(object sender, RoutedEventArgs e)
        {
            _jsonService.SaveShoppingList(Items);
            UpdateSummary();
        }

        private void ArchiveSingleItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ShoppingItem item })
            {
                item.IsPurchased = true;
                item.ArchivedAt = DateTime.Now;

                item.PropertyChanged -= Item_PropertyChanged;
                Items.Remove(item);

                var archive = _jsonService.LoadArchive();
                archive.Insert(0, item);
                _jsonService.SaveArchive(archive);
                _jsonService.SaveShoppingList(Items);

                UpdateSummary();

                MessageBox.Show($"Товар «{item.Name}» успішно перенесено в архів покупок.", "Архівування", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ArchivePurchased_Click(object sender, RoutedEventArgs e)
        {
            int count = _jsonService.ArchivePurchasedItems(Items);
            if (count > 0)
            {
                UpdateSummary();
                MessageBox.Show($"Успішно перенесено в архів покупок {count} товар(ів).", "Архів покупок", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearList_Click(object sender, RoutedEventArgs e)
        {
            if (Items.Count == 0) return;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити всі товари зі списку?",
                "Очищення списку",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in Items)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
                Items.Clear();
                _jsonService.SaveShoppingList(Items);
                UpdateSummary();
            }
        }
    }
}
