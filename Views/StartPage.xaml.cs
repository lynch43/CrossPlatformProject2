using CrossPlatformProject2.ViewModels;

namespace CrossPlatformProject2.Views;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
        BindingContext = new StartPageViewModel();
    }
}
