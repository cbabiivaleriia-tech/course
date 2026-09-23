using System.Windows;
using System.Windows.Controls;
using WeeklyShoppingListWPF.Services;

namespace WeeklyShoppingListWPF.Views
{
    public partial class SettingsPage : Page
    {
        private bool _isInitializing = true;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;
            if (App.CurrentTheme == "Light")
            {
                LightThemeRadioButton.IsChecked = true;
            }
            else
            {
                DarkThemeRadioButton.IsChecked = true;
            }
            _isInitializing = false;

            LoadProfileInfo();
        }

        private void LoadProfileInfo()
        {
            var user = AuthService.CurrentUser;
            if (user != null)
            {
                ProfileNameTextBlock.Text = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
                ProfileUsernameTextBlock.Text = $"@{user.Username}";
                ProfileBudgetTextBlock.Text = user.BudgetLimit > 0 ? $"{user.BudgetLimit:N2} грн" : "Не встановлено";
                ProfileCreatedTextBlock.Text = user.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                ProfileNameTextBlock.Text = "Гість";
                ProfileUsernameTextBlock.Text = "—";
                ProfileBudgetTextBlock.Text = "—";
                ProfileCreatedTextBlock.Text = "—";
            }
        }

        private void LightTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            if (Application.Current is App app)
            {
                app.ChangeTheme("Light");
            }
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            if (Application.Current is App app)
            {
                app.ChangeTheme("Dark");
            }
        }
    }
}
