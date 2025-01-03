using CrossPlatformProject2.ViewModels;

namespace CrossPlatformProject2.Views;

public partial class GamePage : ContentPage
{
    public GamePage(List<string> playerNames)
    {
        InitializeComponent();

        // Set the BindingContext to a new instance of GamePageViewModel with player names
        BindingContext = new GamePageViewModel(playerNames);
    }
}
