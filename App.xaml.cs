using CrossPlatformProject2.Views;

namespace CrossPlatformProject2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new StartPage());
        }
    }
}
