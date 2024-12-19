namespace CrossPlatformProject2.Views;

public partial class StartPage : ContentPage
{
    public StartPage()
    {
        InitializeComponent();
    }

    private void btnSettings_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    private async void btnStartTrivia_Clicked(object sender, EventArgs e)
    {
        var playerNames = new List<string>
        {
            player1Entry.Text,
            player2Entry.Text,
            player3Entry.Text,
            player4Entry.Text
        }.Where(name => !string.IsNullOrEmpty(name)).ToList();

        if (playerNames.Count > 0 && playerNames.Count <= 4)
        {
           
            var gamePage = new GamePage(playerNames);
            await Navigation.PushAsync(gamePage);
        }
        else
        {
            await DisplayAlert("Invalid Input", "Please enter valid player names (1-4).", "OK");
        }
    }
}