using CrossPlatformProject2.ViewModels;

namespace CrossPlatformProject2.Views;

public partial class GamePage : ContentPage
{
    public GamePage(List<string> playerNames, string difficulty, int numberOfRounds)
    {
        InitializeComponent();
        BindingContext = new GamePageViewModel(playerNames, difficulty, numberOfRounds);
    }
}

