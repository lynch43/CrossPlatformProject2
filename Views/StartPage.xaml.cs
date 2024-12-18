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

    private void btnStartTrivia_Clicked(object sender, EventArgs e)
    {
		Shell.Current.GoToAsync(nameof(GamePage));
    }
}