using CrossPlatformProject2.Views;

namespace CrossPlatformProject2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();


            Routing.RegisterRoute(nameof(StartPage), typeof(StartPage));
            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }
    }
}
