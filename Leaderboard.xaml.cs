using CrossPlatformProject2.ViewModels;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace CrossPlatformProject2
{
    public partial class Leaderboard : ContentPage
    {
        private LeaderboardViewModel viewModel;
        private MainPage mainPage;

        public Leaderboard(MainPage mainPage, Dictionary<string, int> playerScores)
        {
            InitializeComponent();
            this.mainPage = mainPage; // Store the reference to MainPage

            // Initialize ViewModel with dynamic player scores
            viewModel = new LeaderboardViewModel(playerScores);
            BindingContext = viewModel; // Set the BindingContext to the ViewModel
        }

        private void homeButton_Clicked(object sender, EventArgs e)
        {
            // Restart rotation or any other logic in MainPage
            //mainPage.startRotation();
            Navigation.PopAsync(); // Navigate back
        }

        private async void resetButton_Clicked(object sender, EventArgs e)
        {
            bool isConfirmed = await DisplayAlert("Confirm Reset", "Are you sure you want to reset the leaderboard?", "Yes", "No");
            if (isConfirmed)
            {
                try
                {
                    viewModel.ResetLeaderboard();
                    await DisplayAlert("Success", "Leaderboard has been reset.", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting leaderboard: {ex.Message}");
                    await DisplayAlert("Error", "An error occurred while resetting the leaderboard.", "OK");
                }
            }
        }
    }
}