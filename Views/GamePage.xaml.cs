using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossPlatformProject2.Views
{
    public partial class GamePage : ContentPage
    {
        private const string ApiUrl = "https://opentdb.com/api.php?amount=10&category=22&difficulty=medium&type=multiple";
        private List<TriviaQuestion> triviaQuestions = new();
        private int currentQuestionIndex;
        private Dictionary<string, int> playerScores = new();
        private List<string> playerNames = new();
        private int currentPlayerIndex;

        public GamePage()
        {
            InitializeComponent();
        }

        public GamePage(List<string> playerNames) : this()
        {
            this.playerNames = playerNames;
            foreach (var name in playerNames)
            {
                playerScores[name] = 0;
            }
            FetchTriviaQuestions();
        }

        public void SetPlayerNames(List<string> playerNames)
        {
            this.playerNames = playerNames;
            playerScores.Clear();
            foreach (var name in playerNames)
            {
                playerScores[name] = 0;
            }
            currentPlayerIndex = 0;
            currentQuestionIndex = 0;
        }

        private async void FetchTriviaQuestions()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(ApiUrl);
                var triviaData = JsonSerializer.Deserialize<TriviaResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (triviaData?.Results?.Count > 0)
                {
                    triviaQuestions = triviaData.Results;
                    DisplayQuestion();
                }
                else
                {
                    await DisplayAlert("No Questions", "Unable to fetch trivia questions.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load trivia questions: {ex.Message}", "OK");
            }
        }

        private void DisplayQuestion()
        {
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                var currentQuestion = triviaQuestions[currentQuestionIndex];
                QuestionLabel.Text = WebUtility.HtmlDecode(currentQuestion.Question);
                var allAnswers = new List<string>(currentQuestion.IncorrectAnswers)
                {
                    currentQuestion.CorrectAnswer
                };
                allAnswers = allAnswers.OrderBy(_ => Guid.NewGuid()).ToList();
                Answer1.Content = WebUtility.HtmlDecode(allAnswers[0]);
                Answer2.Content = WebUtility.HtmlDecode(allAnswers[1]);
                Answer3.Content = WebUtility.HtmlDecode(allAnswers[2]);
                Answer4.Content = WebUtility.HtmlDecode(allAnswers[3]);
                Answer1.IsChecked = false;
                Answer2.IsChecked = false;
                Answer3.IsChecked = false;
                Answer4.IsChecked = false;
                UpdateScoreLabel();
            }
            else
            {
                QuestionLabel.Text = "Quiz Complete!";
                var finalScores = string.Join(Environment.NewLine, playerScores.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                DisplayAlert("Quiz Finished", $"Final Scores:\n{finalScores}", "OK");
            }
        }

        private async void OnSubmitAnswerClicked(object sender, EventArgs e)
        {
            if (playerNames == null || playerNames.Count == 0)
            {
                await DisplayAlert("Error", "No players are available.", "OK");
                return;
            }
            var selectedAnswer = new[] { Answer1, Answer2, Answer3, Answer4 }
                .FirstOrDefault(r => r.IsChecked)
                ?.Content?.ToString();
            if (string.IsNullOrEmpty(selectedAnswer))
            {
                await DisplayAlert("No Selection", "Please select an answer before submitting.", "OK");
                return;
            }
            var currentQuestion = triviaQuestions[currentQuestionIndex];
            var currentPlayer = playerNames[currentPlayerIndex];
            if (selectedAnswer == currentQuestion.CorrectAnswer)
            {
                playerScores[currentPlayer]++;
                await DisplayAlert("Correct", $"{currentPlayer} answered correctly!", "OK");
            }
            else
            {
                await DisplayAlert("Incorrect", $"The correct answer was: {currentQuestion.CorrectAnswer}", "OK");
            }
            currentQuestionIndex++;
            currentPlayerIndex = (currentPlayerIndex + 1) % playerNames.Count;
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                DisplayQuestion();
            }
            else
            {
                QuestionLabel.Text = "Quiz Complete!";
                var finalScores = string.Join(Environment.NewLine, playerScores.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                await DisplayAlert("Quiz Finished", $"Final Scores:\n{finalScores}", "OK");
            }
            UpdateScoreLabel();
        }

        private void UpdateScoreLabel()
        {
            if (playerScores.Count == 0)
            {
                ScoreLabel.Text = "No players available";
                TurnLabel.Text = string.Empty;
            }
            else
            {
                var scores = string.Join(", ", playerScores.Select(ps => $"{ps.Key}: {ps.Value}"));
                ScoreLabel.Text = $"Scores -> {scores}";
                if (currentQuestionIndex < triviaQuestions.Count)
                {
                    TurnLabel.Text = $"{playerNames[currentPlayerIndex]}'s turn";
                }
                else
                {
                    TurnLabel.Text = "Quiz finished";
                }
            }
        }
    }

    public class TriviaResponse
    {
        [JsonPropertyName("results")]
        public List<TriviaQuestion> Results { get; set; }
    }

    public class TriviaQuestion
    {
        [JsonPropertyName("question")]
        public string Question { get; set; }
        [JsonPropertyName("correct_answer")]
        public string CorrectAnswer { get; set; }
        [JsonPropertyName("incorrect_answers")]
        public List<string> IncorrectAnswers { get; set; }
    }
}
