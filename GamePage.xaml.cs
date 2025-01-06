using CrossPlatformProject2.Models; // Implement models to GamePage 
using Newtonsoft.Json; // JSON deserializer
using System.Net;
using CrossPlatformProject2.ViewModels;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Android.App;
using static Android.Provider.DocumentsContract;

namespace CrossPlatformProject2
{
    public partial class GamePage : ContentPage
    {
        private int score;


        private string selectedPlayers { get; set; } // Selected player
        private string selectedDifficulty { get; set; } // Difficulty
        private string selectedCategory { get; set; } // Category
        private int selectedCategoryID { get; set; }
        private int totalQuestions { get; set; } // Holds the number of questions to ask

        private List<string> playerNames { get; set; } // Stores player names

        string apiUrl => $"https://opentdb.com/api.php?amount={totalQuestions}&category={selectedCategoryID}&difficulty={selectedDifficulty.ToLower()}"; // API URL 

        private int currentPlayerIndex = 0; // Keep track of current player

        private Dictionary<string, int> playerScores = new(); // Track the players' scores 

        public static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "SavedGame.json"); // File path

        private AchievementsViewModel achievementsViewModel; // Instantiate ViewModel

        private IDispatcherTimer questionTimer; // Timer to handle question time limits and countdown functionality

        // Tracks the remaining time for the current question
        private int timeRemaining;

        // Indicates whether the timer is currently running
        private bool isTimerRunning;

        private List<QuestionModel> triviaQuestions = new List<QuestionModel>(); // Stores fetched trivia questions
        private int currentQuestionIndex = 0; // Keep track of question

        public GamePage(string selectedPlayers, string selectedDifficulty, int selectedCategoryId, List<string> playerNames, int totalQuestions, GameState gameState = null)
        {
            InitializeComponent();
            InitializeTimer();
            achievementsViewModel = new AchievementsViewModel();
            int totalQuestionsForGame = totalQuestions * playerNames.Count; // Total questions based on players and selected number of questions per player
            this.totalQuestions = totalQuestionsForGame;


            if (gameState != null)
            {
                // Load from game state 
                this.selectedPlayers = gameState.SelectedPlayers;
                this.selectedDifficulty = gameState.SelectedDifficulty;
                // this.selectedCategory = gameState.SelectedCategory;
                this.selectedCategoryID = gameState.selectedCategoryId;
                this.playerNames = gameState.PlayerNames;
                this.triviaQuestions = gameState.TriviaQuestions;
                this.currentQuestionIndex = gameState.CurrentQuestionIndex;
                this.currentPlayerIndex = gameState.CurrentPlayerIndex;
                this.playerScores = gameState.PlayerScores;

                DisplayQuestion(); // Display next question
            }
            else
            {
                // Start new game
                this.selectedCategoryID = selectedCategoryId;
                this.selectedPlayers = selectedPlayers;
                this.selectedDifficulty = selectedDifficulty;
                this.playerNames = playerNames;

                foreach (var player in playerNames)
                {
                    playerScores[player] = 0;
                }

                // Load from the API 
                _ = InitializeGameAsync(); // Asynchronously loads game, as first question after loading appearing blank
            }
        }

        // Override the OnDisappearing method to ensure the timer stops when the page is no longer visible
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopTimer(); // Stops the timer to avoid unintended behavior
        }

        // Initializes the timer for question countdown
        private void InitializeTimer()
        {
            // Create a timer using the application's dispatcher
            questionTimer = Application.Current.Dispatcher.CreateTimer();

            // Set the timer interval to 1 second
            questionTimer.Interval = TimeSpan.FromSeconds(1);

            // Attach the Tick event handler for timer functionality
            questionTimer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // See if there is remaining time
            if (timeRemaining > 0)
            {
                timeRemaining--; // Decrement the remaining time by 1 second

                // Update the timer label to show the remaining time
                timerLabel.Text = $"Time Remaining: {timeRemaining}s";

                // Change text color to red if time is less than or equal to 5 seconds
                if (timeRemaining <= 5)
                {
                    timerLabel.TextColor = Colors.Red;
                }
            }
            else
            {
                // Call the TimeUp method when time runs out
                TimeUp();
            }
        }

        private void StartTimer()
        {
            // Disable the timer for "Easy" difficulty
            if (selectedDifficulty == "Easy")
            {
                timerLabel.IsVisible = false; // Hide the timer label
                return; // Exit the method as no timer is needed
            }

            // Set the initial time based on the selected difficulty
            timeRemaining = selectedDifficulty switch
            {
                "Medium" => 20, // 20 seconds for Medium
                "Hard" => 15,   // 15 seconds for Hard
                _ => 0
            };

            // Set the timer label properties
            timerLabel.TextColor = Colors.Gold; // Default text color
            timerLabel.IsVisible = true; // Make the timer label visible
            timerLabel.Text = $"Time Remaining: {timeRemaining}s"; // Display the initial time

            // Start timer
            questionTimer.Start();
            isTimerRunning = true; // Set timer state to running
        }

        // Stops the timer if it is currently running
        private void StopTimer()
        {
            if (isTimerRunning)
            {
                questionTimer.Stop(); // Stop the timer
                isTimerRunning = false; // Update the timer state
            }
        }

        // Handles the event when time runs out for the current player
        private async void TimeUp()
        {
            // Stop the timer to prevent further updates
            StopTimer();
            UpdateCurrentPlayerLabel();
            await DisplayAlert("Time's Up!", $"{playerNames[currentPlayerIndex]}'s time has run out!", "OK"); // Notify the user that the current player's time has run out

            // Move to the next player in a circular fashion
            currentPlayerIndex = (currentPlayerIndex + 1) % playerNames.Count;

            currentQuestionIndex++; // Move to the next question

            // Check if there are more questions to display
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                DisplayQuestion(); // Display the next question
            }
            else
            {
                // End the game if there are no more questions
                await endGame();
            }
        }

        private async Task InitializeGameAsync()
        {
            await LoadQuestionsFromApi(selectedCategoryID, selectedDifficulty);

            if (triviaQuestions.Any())
            {
                triviaQuestions = triviaQuestions.Take(totalQuestions).ToList();
                DisplayQuestion(); // Questions displayed after being loaded
                UpdateCurrentPlayerLabel();
            }
            else
            {
                await DisplayAlert("Error", "Failed to load questions. Please try again.", "OK");
                await Navigation.PopToRootAsync(); // If no options available, returns to main page 
            }
        }

        private async Task LoadQuestionsFromApi(int selectedCategoryID, string selectedDifficulty)
        {
            try
            {
                using HttpClient client = new HttpClient();
                var response = await client.GetStringAsync(apiUrl);

                // Async request to get from API
                var root = JsonConvert.DeserializeObject<Root>(response);

                if (root?.response_code == 0 && root.results?.Count > 0) // Ensures root and properties not null
                {
                    triviaQuestions = root.results.Select(result => new QuestionModel // Populate trivia questions list with data from API
                    {
                        // Decode any HTML
                        Question = WebUtility.HtmlDecode(result.question),
                        CorrectAnswer = WebUtility.HtmlDecode(result.correct_answer),
                        IncorrectAnswers = result.incorrect_answers.Select(WebUtility.HtmlDecode).ToList()
                    }).ToList();

                    totalQuestions = triviaQuestions.Count;

                    // DisplayQuestion();
                }
                else // Display in case of error
                {
                    await DisplayAlert("Error", "No questions available for the selected options.", "OK");
                }
            }
            catch (Exception ex) // Display alert if questions not loaded
            {
                await DisplayAlert("Error", $"Failed to load questions: {ex.Message}", "OK");
            }
        }

        private async void DisplayQuestion()
        {
            StopTimer();
            // Ensure index is within range 
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                var question = triviaQuestions[currentQuestionIndex]; // Retrieve question object from the list

                // Check if question data is complete
                if (string.IsNullOrWhiteSpace(question.Question) ||
                    string.IsNullOrWhiteSpace(question.CorrectAnswer) ||
                    question.IncorrectAnswers == null || question.IncorrectAnswers.Count != 3)
                {
                    // await DisplayAlert("Error", "Incomplete question data. Skipping to the next question.", "OK");
                    currentQuestionIndex++;
                    if (currentQuestionIndex < triviaQuestions.Count)
                        DisplayQuestion();
                    else
                        await endGame();
                    return;
                }

                // Decode HTML coded questions
                questionLabel.Text = WebUtility.HtmlDecode(question.Question);

                // Make a list with correct and incorrect answers
                var answers = question.IncorrectAnswers
                    .Concat(new[] { question.CorrectAnswer }) // Add correct answer
                    .OrderBy(_ => Guid.NewGuid()) // Shuffle answers
                    .ToList(); // Add result to list

                // Display shuffled answers on the buttons
                if (answers.Count == 4)
                {
                    answerButton1.Text = answers[0];
                    answerButton2.Text = answers[1];
                    answerButton3.Text = answers[2];
                    answerButton4.Text = answers[3];
                    StartTimer();
                }
            }
            else
            {
                await DisplayAlert("Error", "Invalid question data detected. Skipping to the next question.", "OK");
                currentQuestionIndex++;
                if (currentQuestionIndex < triviaQuestions.Count)
                    DisplayQuestion();
                else
                    await endGame();
            }
        }

        private async Task endGame()
        {
            StopTimer();
            // Score message for end of game
            string scoresMessage = "Game Over! Here are the final scores:\n\n";

            foreach (var playerScore in playerScores)
            {
                scoresMessage += $"{playerScore.Key}: {playerScore.Value} points\n";
            }

            // Get highest score
            int highestScore = playerScores.Values.Max();

            // Determine winner
            var winners = playerScores
                .Where(p => p.Value == highestScore) // Where highest score
                .Select(p => p.Key) // Get player name
                .ToList();

            if (winners.Count == 1)
            {
                scoresMessage += $"\nWinner: {winners[0]} with {highestScore} points!";
            }
            else
            {
                scoresMessage += $"\nIt's a tie! Winners: {string.Join(", ", winners)} with {highestScore} points!";
            }

            // Display the final results with animation
            await ShowFinalResultsAsync(scoresMessage);

            // Save player scores globally
            ((App)Application.Current).PlayerScores = playerScores;

            CheckAndUnlockAchievements(); // Check and unlock achievements after every question

            // Optionally, save the game progress
            await SaveGameToFile();
        }

        private async Task ShowFinalResultsAsync(string scoresMessage)
        {
            FinalScoresLabel.Text = scoresMessage;
            FinalResultsOverlay.IsVisible = true;

            // Reset the scale and opacity
            FinalResultsOverlay.Opacity = 0;
            FinalResultsOverlay.Scale = 0.8;

            // Animate opacity and scale to make it appear
            await Task.WhenAll(
                FinalResultsOverlay.FadeTo(1, 500, Easing.CubicOut),
                FinalResultsOverlay.ScaleTo(1, 500, Easing.CubicOut)
            );
        }

        private async Task HideFinalResultsAsync()
        {
            // Animate opacity and scale to make it disappear
            await Task.WhenAll(
                FinalResultsOverlay.FadeTo(0, 300, Easing.CubicIn),
                FinalResultsOverlay.ScaleTo(0.8, 300, Easing.CubicIn)
            );

            FinalResultsOverlay.IsVisible = false;
        }

        // Updated Event Handlers for the New Buttons
        private async void OnPlayAgainClicked(object sender, EventArgs e)
        {
            await HideFinalResultsAsync();

            // Restart the game (reset game state and navigate to GamePage)
            await Navigation.PushAsync(new GamePage(selectedPlayers, selectedDifficulty, selectedCategoryID, playerNames, totalQuestions));
        }

        private async void OnQuitClicked(object sender, EventArgs e)
        {
            await HideFinalResultsAsync();

            // Navigate to the main page or leaderboard
            await Navigation.PopToRootAsync();
        }

        private async void OnAnswerClicked(object sender, EventArgs e)
        {
            StopTimer();
            // Identify which button was clicked
            Button clickedButton = sender as Button; // Sender is the button that was clicked
            // Cast sender to button, to access its properties

            if (clickedButton == null)
            {
                return;
            }

            var currentQuestion = triviaQuestions[currentQuestionIndex]; // Retrieve current index of question and question
            string currentPlayerName = playerNames[currentPlayerIndex]; // Retrieves the name of the player whose turn it currently is

            // Check if answer is correct
            // Compare button text to correct answer, placeholder for now 
            if (clickedButton.Text == currentQuestion.CorrectAnswer)
            {
                // Calculate points based on difficulty
                int points = selectedDifficulty switch
                {
                    "Easy" => 2,
                    "Medium" => 6,
                    "Hard" => 10,
                    _ => 0 // Default
                };

                // Updated score for player
                await CorrectAnswerAnimation(clickedButton);
                playerScores[currentPlayerName] += points;
                // DisplayAlert("Correct!", $"{currentPlayerName} got it right and earned {points} points!", "Next"); // Display alert now shows points earned

            }
            else
            {
                await IncorrectAnswerAnimation(clickedButton);
                // Display for incorrect answer
                // DisplayAlert("Wrong!!", "Better luck next time", "Ok");
            }

            currentQuestionIndex++; // Increment index of question
            currentPlayerIndex = (currentPlayerIndex + 1) % playerNames.Count; // Move to next player in the list
            UpdateCurrentPlayerLabel(); // Update Player Turns name
            if (currentQuestionIndex < triviaQuestions.Count)
                DisplayQuestion();
            else
                await endGame();
        }


        private async Task CorrectAnswerAnimation(Button clickedButton)
        {
            // Scale up the correct answer button for a "correct" animation
            await clickedButton.ScaleTo(1.2, 400); // Scale up by 20% in 400ms
            await clickedButton.ScaleTo(1, 400);   // Scale back to original size
            clickedButton.BackgroundColor = Colors.Green; // Change background color to green to indicate correct
            await Task.Delay(500); // Wait for half a second before resetting
            clickedButton.BackgroundColor = Colors.Gray; // Reset background color
        }

        private async Task IncorrectAnswerAnimation(Button clickedButton)
        {
            // Change the background color to red to indicate an incorrect answer
            clickedButton.BackgroundColor = Colors.Red;

            // Perform a shaking animation by alternating left and right positions
            for (int i = 0; i < 3; i++)
            {
                // Shake to the left
                await clickedButton.TranslateTo(-10, 0, 100, Easing.Linear);
                // Shake to the right
                await clickedButton.TranslateTo(10, 0, 100, Easing.Linear);
            }

            // Return to the original position
            await clickedButton.TranslateTo(0, 0, 100, Easing.Linear);

            // Wait for a moment to let the feedback be visible
            await Task.Delay(500);

            // Reset the button style
            clickedButton.BackgroundColor = Colors.Gray;
        }

        private void UpdateCurrentPlayerLabel()
        {
            currentPlayerLabel.Text = $"{playerNames[currentPlayerIndex]}'s Turn";
        }

        private async Task SaveGameToFile() // Save the game state to file 
        {
            try
            {
                var gameState = new GameState
                {
                    PlayerScores = playerScores,
                    CurrentQuestionIndex = currentQuestionIndex,
                    TriviaQuestions = triviaQuestions, // Save questions
                    CurrentPlayerIndex = currentPlayerIndex,
                    SelectedDifficulty = selectedDifficulty,
                    SelectedCategory = selectedCategory,
                    PlayerNames = playerNames
                };
                // Save game state
                var gameStateJson = JsonConvert.SerializeObject(gameState);
                await File.WriteAllTextAsync(FilePath, gameStateJson);

                // await DisplayAlert("Save Game", "Game progress saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save game: {ex.Message}", "OK");
            }
        }

        public void CheckAndUnlockAchievements() // Method to unlock achievements 
        {
            int currentScore = playerScores[playerNames[currentPlayerIndex]];

            achievementsViewModel.UpdateAchievements(currentScore); // Update achievements in the ViewModel

            foreach (var achievement in achievementsViewModel.Achievements) // For each achievement in the ViewModel
            {
                if (!achievement.IsUnlocked && currentScore >= achievement.PointThreshold)
                {
                    achievement.IsUnlocked = true; // Achievement unlocked true
                    DisplayAlert("Achievement Unlocked!", $"Congratulations! You unlocked: {achievement.Title}", "OK"); // Message display
                }
            }

            // Save the updated achievements
            achievementsViewModel.SaveAchievementsToFile();
        }

        private async void OnSaveGameClicked(object sender, EventArgs e)
        {
            try
            {
                // Save game
                await SaveGameToFile();

                // Display game saved message
                await DisplayAlert("Save Game", "Game progress saved successfully.", "OK");

                // Take user back to home page
                await Navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                // Error handling
                await DisplayAlert("Error", $"Failed to save the game: {ex.Message}", "OK");
            }
        }
    }
}
