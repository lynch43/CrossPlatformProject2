using CrossPlatformProject2.Services;
using System.Windows.Input;

namespace CrossPlatformProject2.ViewModels;

public class SettingsPageViewModel : BaseViewModel
{
    private string _welcomeMessage = "Customize settings for the Game";
    private int _numberOfPlayers = SettingsService.Instance.NumberOfPlayers;
    private int _difficultyIndex = SettingsService.Instance.Difficulty switch
    {
        "Easy" => 0,
        "Medium" => 1,
        "Hard" => 2,
        _ => 1
    };
    private int _numberOfRounds = SettingsService.Instance.NumberOfRounds;

    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    public int NumberOfPlayers
    {
        get => _numberOfPlayers;
        set => SetProperty(ref _numberOfPlayers, value);
    }

    public int DifficultyIndex
    {
        get => _difficultyIndex;
        set
        {
            if (SetProperty(ref _difficultyIndex, value))
            {
                Difficulty = value switch
                {
                    0 => "Easy",
                    1 => "Medium",
                    2 => "Hard",
                    _ => "Medium"
                };
            }
        }
    }

    private string _difficulty = SettingsService.Instance.Difficulty;
    public string Difficulty
    {
        get => _difficulty;
        private set => SetProperty(ref _difficulty, value);
    }

    public int NumberOfRounds
    {
        get => _numberOfRounds;
        set => SetProperty(ref _numberOfRounds, value);
    }

    public ICommand SaveSettingsCommand { get; }

    public SettingsPageViewModel()
    {
        SaveSettingsCommand = new Command(OnSaveSettings);
    }

    private async void OnSaveSettings()
    {
        try
        {
            var settings = SettingsService.Instance;
            settings.NumberOfPlayers = NumberOfPlayers;
            settings.Difficulty = Difficulty;
            settings.NumberOfRounds = NumberOfRounds;

            await Application.Current.MainPage.DisplayAlert("Settings Saved",
                $"Players: {NumberOfPlayers}\nDifficulty: {Difficulty}\nRounds: {NumberOfRounds}",
                "OK");

            await Application.Current.MainPage.Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }
}
