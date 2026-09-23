using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WeeklyShoppingListWPF.Services;
using WeeklyShoppingListWPF.Views;

namespace WeeklyShoppingListWPF
{
    public partial class MainWindow : Window
    {
        private readonly AuthService _authService = new AuthService();
        private Button? _activeNavButton;

        public MainWindow()
        {
            InitializeComponent();
            AuthService.CurrentUserChanged += UpdateUserProfile;
            UpdateUserProfile();

            // При запуску відкриваємо головну сторінку
            NavigateTo(new HomePage(), HomeNavButton);
        }

        private void UpdateUserProfile()
        {
            Dispatcher.Invoke(() =>
            {
                var user = AuthService.CurrentUser;
                if (user != null)
                {
                    UserDisplayNameTextBlock.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
                    UsernameTextBlock.Text = $"@{user.Username}";

                    string initial = !string.IsNullOrWhiteSpace(user.FullName)
                        ? user.FullName.Trim().Substring(0, 1).ToUpper()
                        : user.Username.Trim().Substring(0, 1).ToUpper();

                    UserInitialTextBlock.Text = initial;
                }
                else
                {
                    UserDisplayNameTextBlock.Text = "Гість";
                    UsernameTextBlock.Text = "@guest";
                    UserInitialTextBlock.Text = "👤";
                }
            });
        }

        private void NavigateTo(Page page, Button navButton)
        {
            MainFrame.Navigate(page);
            HighlightNavButton(navButton);
        }

        private void HighlightNavButton(Button activeButton)
        {
            if (_activeNavButton != null)
            {
                _activeNavButton.Background = Brushes.Transparent;
                _activeNavButton.Foreground = (Brush)FindResource("SecondaryTextBrush");
            }

            _activeNavButton = activeButton;
            if (_activeNavButton != null)
            {
                _activeNavButton.Background = (Brush)FindResource("BadgeBackgroundBrush");
                _activeNavButton.Foreground = (Brush)FindResource("TextBrush");
            }
        }

        // Головна
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new HomePage(), HomeNavButton);
        }

        // Список покупок
        private void ShoppingListButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new ShoppingListPage(), ShoppingListNavButton);
        }

        // Архів покупок
        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new ArchivePage(), ArchiveNavButton);
        }

        // Налаштування
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new SettingsPage(), SettingsNavButton);
        }

        // Вихід з акаунта
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Ви дійсно бажаєте вийти з облікового запису?",
                "Вихід із системи",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();

                Hide();
                var loginDialog = new LoginWindow();
                bool? loginResult = loginDialog.ShowDialog();

                if (loginResult == true && AuthService.CurrentUser != null)
                {
                    UpdateUserProfile();
                    NavigateTo(new HomePage(), HomeNavButton);
                    Show();
                }
                else
                {
                    Close();
                }
            }
        }
    }
}