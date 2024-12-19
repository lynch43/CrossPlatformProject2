using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossPlatformProject2.Views;

public partial class GamePage : ContentPage
{
    private const string ApiUrl = "https://opentdb.com/api.php?amount=10&category=20&difficulty=easy&type=multiple";
    private List<TriviaQuestion> triviaQuestions = new();
    private int currentQuestionIndex = 0;
    private int score = 0;
    private List<string> playerNames = new();
    private int currentPlayerIndex = 0;


    public GamePage()
    {
        InitializeComponent();
    }

    public GamePage(List<string> playerNames) : this()
    {
        this.playerNames = playerNames;
        UpdateScoreLabel();
        FetchTriviaQuestions();
    }


    public void SetPlayerNames(List<string> playerNames)
    {
        this.playerNames = playerNames;
        UpdateScoreLabel();
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

    private async void DisplayQuestion()
    {
        if (currentQuestionIndex < triviaQuestions.Count)
        {
            var currentQuestion = triviaQuestions[currentQuestionIndex];
            QuestionLabel.Text = currentQuestion.Question;

            var answers = new List<string>(currentQuestion.IncorrectAnswers) { currentQuestion.CorrectAnswer };
            answers = answers.OrderBy(_ => Guid.NewGuid()).ToList();

            Answer1.Content = answers[0];
            Answer2.Content = answers[1];
            Answer3.Content = answers[2];
            Answer4.Content = answers[3];

            UpdateScoreLabel();
        }
        else
        {
            QuestionLabel.Text = "Quiz Complete!";
            await DisplayAlert("Quiz Finished", $"Your score: {score}/{triviaQuestions.Count}", "OK");
        }
    }

    private async void OnSubmitAnswerClicked(object sender, EventArgs e)
    {
        
        if (playerNames.Count == 0)
        {
            await DisplayAlert("Error", "No players available.", "OK");
            return;
        }

        
        var selectedAnswer = new[] { Answer1, Answer2, Answer3, Answer4 }
            .FirstOrDefault(r => r.IsChecked)?.Content?.ToString();

        if (string.IsNullOrEmpty(selectedAnswer))
        {
            await DisplayAlert("No Selection", "Please select an answer before submitting.", "OK");
            return;
        }

        var currentQuestion = triviaQuestions[currentQuestionIndex];

        // Check if the answer is correct
        if (selectedAnswer == currentQuestion.CorrectAnswer)
        {
            score++;
            await DisplayAlert("Correct", "You answered correctly!", "OK");
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
            await DisplayAlert("Quiz Finished", $"Your score: {score}/{triviaQuestions.Count}", "OK");
        }

        
        UpdateScoreLabel();
    }

    private void UpdateScoreLabel()
    {
        ScoreLabel.Text = $"Score: {score}/{triviaQuestions.Count} - {playerNames[currentPlayerIndex]}'s turn";
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