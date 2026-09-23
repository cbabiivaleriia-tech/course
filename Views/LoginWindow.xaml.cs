using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WeeklyShoppingListWPF.Services;

namespace WeeklyShoppingListWPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new AuthService();

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => LoginUsernameBox.Focus();
        }

        private void TabLogin_Click(object sender, RoutedEventArgs e)
        {
            SwitchToLogin();
        }

        private void TabRegister_Click(object sender, RoutedEventArgs e)
        {
            SwitchToRegister();
        }

        private void SwitchToLogin()
        {
            LoginFormPanel.Visibility = Visibility.Visible;
            RegisterFormPanel.Visibility = Visibility.Collapsed;

            TabLoginButton.Style = (Style)FindResource("PrimaryButtonStyle");
            TabRegisterButton.Style = null;
            TabRegisterButton.Background = Brushes.Transparent;
            TabRegisterButton.Foreground = (Brush)FindResource("SecondaryTextBrush");

            HideLoginMessage();
            LoginUsernameBox.Focus();
        }

        private void SwitchToRegister()
        {
            LoginFormPanel.Visibility = Visibility.Collapsed;
            RegisterFormPanel.Visibility = Visibility.Visible;

            TabRegisterButton.Style = (Style)FindResource("PrimaryButtonStyle");
            TabLoginButton.Style = null;
            TabLoginButton.Background = Brushes.Transparent;
            TabLoginButton.Foreground = (Brush)FindResource("SecondaryTextBrush");

            HideRegMessage();
            RegUsernameBox.Focus();
        }

        private void LoginSubmit_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void RegisterSubmit_Click(object sender, RoutedEventArgs e)
        {
            PerformRegister();
        }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void RegisterForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformRegister();
            }
        }

        private void PerformLogin()
        {
            string username = LoginUsernameBox.Text?.Trim() ?? string.Empty;
            string password = LoginPasswordBox.Password ?? string.Empty;

            var (success, message) = _authService.Login(username, password);

            if (!success)
            {
                ShowLoginMessage(message, isError: true);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void PerformRegister()
        {
            string username = RegUsernameBox.Text?.Trim() ?? string.Empty;
            string fullName = RegFullNameBox.Text?.Trim() ?? string.Empty;
            string password = RegPasswordBox.Password ?? string.Empty;
            string confirm = RegConfirmPasswordBox.Password ?? string.Empty;

            var (success, message) = _authService.Register(username, fullName, password, confirm);

            if (!success)
            {
                ShowRegMessage(message, isError: true);
                return;
            }

            // Успішно зареєстровано та автоматично авторизовано
            DialogResult = true;
            Close();
        }

        private void ShowLoginMessage(string message, bool isError)
        {
            LoginMessageText.Text = message;
            LoginMessageText.Foreground = (Brush)FindResource(isError ? "DangerBrush" : "SuccessBrush");
            LoginMessageBorder.BorderBrush = (Brush)FindResource(isError ? "DangerBrush" : "SuccessBrush");
            LoginMessageBorder.Visibility = Visibility.Visible;
        }

        private void HideLoginMessage()
        {
            LoginMessageBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowRegMessage(string message, bool isError)
        {
            RegMessageText.Text = message;
            RegMessageText.Foreground = (Brush)FindResource(isError ? "DangerBrush" : "SuccessBrush");
            RegMessageBorder.BorderBrush = (Brush)FindResource(isError ? "DangerBrush" : "SuccessBrush");
            RegMessageBorder.Visibility = Visibility.Visible;
        }

        private void HideRegMessage()
        {
            RegMessageBorder.Visibility = Visibility.Collapsed;
        }
    }
}
