namespace CrossPlatformProject2
{
    public partial class App : Application
    {
        public Dictionary<string, int> PlayerScores { get; set; } = new Dictionary<string, int>();//player scores accessible globaly
        public App()
        {
            InitializeComponent();


            MainPage = new NavigationPage(new MainPage()); //wrap MainPage in NavigationPage
        }
    }
}

