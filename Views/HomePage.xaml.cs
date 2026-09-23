using System.Windows;
using System.Windows.Controls;
using WeeklyShoppingListWPF.Services;

namespace WeeklyShoppingListWPF.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user != null)
            {
                string name = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Username;
                WelcomeTextBlock.Text = $"Ласкаво просимо, {name}!";
            }
            else
            {
                WelcomeTextBlock.Text = "Ласкаво просимо!";
            }
        }

        private void OpenShoppingList_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ShoppingListPage());
        }

        private void OpenArchive_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ArchivePage());
        }
    }
}
