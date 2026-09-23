using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeeklyShoppingListWPF.Models;
using WeeklyShoppingListWPF.Services;

namespace WeeklyShoppingListWPF.Views
{
    public partial class ArchivePage : Page
    {
        private readonly JsonStorageService _jsonService = new JsonStorageService();
        private List<ShoppingItem> _allArchivedItems = new List<ShoppingItem>();

        public ObservableCollection<ShoppingItem> FilteredItems { get; } = new ObservableCollection<ShoppingItem>();

        public ArchivePage()
        {
            InitializeComponent();
            ArchiveItemsControl.ItemsSource = FilteredItems;
            AuthService.CurrentUserChanged += OnCurrentUserChanged;
        }

        private void OnCurrentUserChanged()
        {
            Dispatcher.Invoke(LoadArchiveData);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadArchiveData();
        }

        private void LoadArchiveData()
        {
            _allArchivedItems = _jsonService.LoadArchive();
            ApplyFilter();
            UpdateStats();
        }

        private void ApplyFilter()
        {
            string query = SearchTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;

            FilteredItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allArchivedItems
                : _allArchivedItems.Where(i => 
                    (i.Name?.ToLowerInvariant().Contains(query) ?? false) ||
                    (i.Category?.ToLowerInvariant().Contains(query) ?? false)).ToList();

            foreach (var item in filtered)
            {
                FilteredItems.Add(item);
            }

            if (_allArchivedItems.Count == 0)
            {
                EmptyArchiveBorder.Visibility = Visibility.Visible;
                ArchiveScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyArchiveBorder.Visibility = Visibility.Collapsed;
                ArchiveScrollViewer.Visibility = Visibility.Visible;
            }
        }

        private void UpdateStats()
        {
            decimal totalSpent = _allArchivedItems.Sum(i => i.TotalPrice);
            int count = _allArchivedItems.Count;

            ArchiveTotalSpentTextBlock.Text = $"{totalSpent:N2} грн";
            ArchiveCountTextBlock.Text = $"{count} позицій";

            var latest = _allArchivedItems.OrderByDescending(i => i.ArchivedAt ?? i.CreatedAt).FirstOrDefault();
            if (latest != null && latest.ArchivedAt.HasValue)
            {
                ArchiveLastDateTextBlock.Text = latest.ArchivedAt.Value.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                ArchiveLastDateTextBlock.Text = count > 0 ? "Нещодавно" : "Немає записів";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void RestoreItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ShoppingItem item })
            {
                _jsonService.RestoreItemFromArchive(item);
                _allArchivedItems.Remove(item);
                ApplyFilter();
                UpdateStats();

                MessageBox.Show($"Товар «{item.Name}» повернуто до списку активних покупок!", "Відновлення товару", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePermanent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ShoppingItem item })
            {
                var result = MessageBox.Show(
                    $"Видалити «{item.Name}» з архіву остаточно?",
                    "Видалення з архіву",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _jsonService.DeleteFromArchive(item.Id);
                    _allArchivedItems.Remove(item);
                    ApplyFilter();
                    UpdateStats();
                }
            }
        }

        private void ClearArchive_Click(object sender, RoutedEventArgs e)
        {
            if (_allArchivedItems.Count == 0) return;

            var result = MessageBox.Show(
                "Ви дійсно бажаєте повністю очистити весь архів покупок?",
                "Очищення архіву",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _jsonService.ClearArchive();
                _allArchivedItems.Clear();
                ApplyFilter();
                UpdateStats();
            }
        }
    }
}
