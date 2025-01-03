using CrossPlatformProject2.Views;
using System.Windows.Input;

namespace CrossPlatformProject2.ViewModels
{
    public class StartPageViewModel : BaseViewModel
    {
        private string _player1;
        private string _player2;
        private string _player3;
        private string _player4;

        public string Player1
        {
            get => _player1;
            set => SetProperty(ref _player1, value);
        }

        public string Player2
        {
            get => _player2;
            set => SetProperty(ref _player2, value);
        }

        public string Player3
        {
            get => _player3;
            set => SetProperty(ref _player3, value);
        }

        public string Player4
        {
            get => _player4;
            set => SetProperty(ref _player4, value);
        }

        public ICommand NavigateToSettingsCommand { get; }
        public ICommand StartTriviaCommand { get; }

        public StartPageViewModel()
        {
            NavigateToSettingsCommand = new Command(OnNavigateToSettings);
            StartTriviaCommand = new Command(OnStartTrivia);
        }

        private async void OnNavigateToSettings()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new SettingsPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
            }
        }

        private async void OnStartTrivia()
        {
            var playerNames = new List<string> { Player1, Player2, Player3, Player4 }
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (playerNames.Count > 0 && playerNames.Count <= 4)
            {
                try
                {
                    // Instantiate GamePage with player names and navigate to it
                    var gamePage = new GamePage(playerNames);
                    await Application.Current.MainPage.Navigation.PushAsync(gamePage);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start game: {ex.Message}", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Invalid Input", "Please enter valid player names (1-4).", "OK");
            }
        }
    }
}
