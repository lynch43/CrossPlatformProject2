namespace CrossPlatformProject2.ViewModels;

public class SettingsPageViewModel : BaseViewModel
{
    private string _welcomeMessage = "Welcome to .NET MAUI!";

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    public SettingsPageViewModel()
    {
        
    }
}

