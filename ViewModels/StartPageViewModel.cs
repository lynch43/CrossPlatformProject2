using CrossPlatformProject2.Views;
using CrossPlatformProject2.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CrossPlatformProject2.ViewModels
{
    public class StartPageViewModel : BaseViewModel
    {
        private ObservableCollection<string> _playerNames = new ObservableCollection<string>();
        private int _numberOfPlayers;

        public ObservableCollection<string> PlayerNames
        {
            get => _playerNames;
            set => SetProperty(ref _playerNames, value);
        }

        public int NumberOfPlayers
        {
            get => _numberOfPlayers;
            set
            {
                if (SetProperty(ref _numberOfPlayers, value))
                {
                    UpdatePlayerInputs(); // Update input fields dynamically
                }
            }
        }

        public ICommand NavigateToSettingsCommand { get; }
        public ICommand StartTriviaCommand { get; }

        public StartPageViewModel()
        {
            // Initialize commands
            NavigateToSettingsCommand = new Command(OnNavigateToSettings);
            StartTriviaCommand = new Command(OnStartTrivia);

            // Load settings if available
            var settings = SettingsService.Instance;
            NumberOfPlayers = settings.NumberOfPlayers; // Get player number from settings

            // Initialize player input fields based on the current number of players
            UpdatePlayerInputs();
        }

        // Update settings based on the slider from SettingsPage
        public void UpdateSettings(int numberOfPlayers)
        {
            NumberOfPlayers = numberOfPlayers;
            UpdatePlayerInputs(); // Update the number of input fields based on selected players
        }

        private async void OnNavigateToSettings()
        {
            try
            {
                // Navigate to SettingsPage
                await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
            }
        }

        private async void OnStartTrivia()
        {
            var playerNames = PlayerNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToList();

            if (playerNames.Count > 0 && playerNames.Count <= NumberOfPlayers)
            {
                try
                {
                    var settings = SettingsService.Instance;
                    var gamePage = new GamePage(playerNames, settings.Difficulty, settings.NumberOfRounds);
                    await Application.Current.MainPage.Navigation.PushAsync(gamePage);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start game: {ex.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Invalid Input", $"Please enter valid player names (1-{NumberOfPlayers}).", "OK");
            }
        }

        private void UpdatePlayerInputs()
        {
            while (PlayerNames.Count < NumberOfPlayers)
            {
                PlayerNames.Add(string.Empty);
            }

            while (PlayerNames.Count > NumberOfPlayers)
            {
                PlayerNames.RemoveAt(PlayerNames.Count - 1);
            }
        }
    }
}
