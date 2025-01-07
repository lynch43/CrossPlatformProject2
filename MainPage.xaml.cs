
namespace CrossPlatformProject2
{
    public partial class MainPage : ContentPage

    {

        public MainPage()
        {
            InitializeComponent();


        }
 private async void Leaderboard_Clicked(object sender, EventArgs e)
        {
            

            //retrieve scores from global context
            var playerScores = ((App)Application.Current).PlayerScores;

            await Navigation.PushAsync(new Leaderboard(this, playerScores));  //pass references so leaderboard can access methods while navigating to leaderboard
        }

        private async void Achievments_Clicked(object sender, EventArgs e)
        {
           
            //await Navigation.PushAsync(new Achievements(this));
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GameSetup());
        }
    }

}