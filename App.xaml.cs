using System;
using System.Windows;
using WeeklyShoppingListWPF.Services;
using WeeklyShoppingListWPF.Views;

namespace WeeklyShoppingListWPF
{
    public partial class App : Application
    {
        public static string CurrentTheme { get; private set; } = "Dark";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginDialog = new LoginWindow();
            bool? result = loginDialog.ShowDialog();

            if (result == true && AuthService.CurrentUser != null)
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        public void ChangeTheme(string theme)
        {
            ResourceDictionary newTheme;

            if (theme == "Light")
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri("Resources/LightTheme.xaml", UriKind.Relative)
                };
                CurrentTheme = "Light";
            }
            else
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri("Resources/DarkTheme.xaml", UriKind.Relative)
                };
                CurrentTheme = "Dark";
            }

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(newTheme);
        }
    }
}
