using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace CrossPlatformProject2.ViewModels
{
    public class GamePageViewModel : BaseViewModel
    {
        private static readonly string[] Indicators = { "A", "B", "C", "D" };
        private string ApiUrl => $"https://opentdb.com/api.php?amount=20&category=15&difficulty={_difficulty.ToLower()}&type=multiple";

        private ObservableCollection<AnswerOption> _answers = new();
        private string _currentQuestion;
        private AnswerOption _selectedAnswer;
        private string _scoreDisplay;
        private string _turnDisplay;
        private int _currentQuestionIndex;
        private int _currentPlayerIndex;
        private int _currentRound;
        private int _numberOfRounds = 10;

        private List<TriviaQuestion> _triviaQuestions = new();
        private Dictionary<string, int> _playerScores = new();
        private ObservableCollection<string> _playerNames = new ObservableCollection<string> { "Player 1", "Player 2" };
        private string _difficulty = "Medium";

        public ObservableCollection<AnswerOption> Answers
        {
            get => _answers;
            set => SetProperty(ref _answers, value);
        }

        public string CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        public AnswerOption SelectedAnswer
        {
            get => _selectedAnswer;
            set
            {
                if (_selectedAnswer != value)
                {
                    if (_selectedAnswer != null)
                        _selectedAnswer.IsSelected = false;

                    _selectedAnswer = value;

                    if (_selectedAnswer != null)
                        _selectedAnswer.IsSelected = true;

                    SetProperty(ref _selectedAnswer, value);
                }
            }
        }

        public string ScoreDisplay
        {
            get => _scoreDisplay;
            set => SetProperty(ref _scoreDisplay, value);
        }

        public string TurnDisplay
        {
            get => _turnDisplay;
            set => SetProperty(ref _turnDisplay, value);
        }

        public ICommand SubmitAnswerCommand { get; }
        public ICommand SelectAnswerCommand { get; }

        // Parameterless constructor for XAML
        public GamePageViewModel()
            : this(new List<string> { "Player 1", "Player 2" }, "Medium", 10)
        {
        }

        public GamePageViewModel(List<string> playerNames, string difficulty, int numberOfRounds)
        {
            SubmitAnswerCommand = new Command(OnSubmitAnswer);
            SelectAnswerCommand = new Command<string>(OnSelectAnswer);

            _playerNames = new ObservableCollection<string>(playerNames);
            _difficulty = difficulty;
            _numberOfRounds = numberOfRounds;

            foreach (var name in _playerNames)
            {
                _playerScores[name] = 0;
            }

            FetchTriviaQuestions();
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

                if (triviaData?.Results != null && triviaData.Results.Count > 0)
                {
                    _triviaQuestions = triviaData.Results;
                    DisplayQuestion();
                }
                else
                {
                    CurrentQuestion = "No questions available.";
                }
            }
            catch
            {
                CurrentQuestion = "Error loading questions.";
            }
        }

        private void DisplayQuestion()
        {
            if (_currentRound < _numberOfRounds && _currentQuestionIndex < _triviaQuestions.Count)
            {
                var currentQuestion = _triviaQuestions[_currentQuestionIndex];

                CurrentQuestion = WebUtility.HtmlDecode(currentQuestion.Question);

                var allAnswers = new List<string>(currentQuestion.IncorrectAnswers)
                {
                    currentQuestion.CorrectAnswer
                };
                allAnswers = allAnswers.OrderBy(_ => Guid.NewGuid()).ToList();

                Answers.Clear();
                for (int i = 0; i < allAnswers.Count; i++)
                {
                    Answers.Add(new AnswerOption
                    {
                        Index = Indicators[i],
                        Text = WebUtility.HtmlDecode(allAnswers[i]),
                        IsSelected = false
                    });
                }

                SelectedAnswer = null;
                UpdateScoreDisplay();
            }
            else
            {
                CurrentQuestion = "Game Complete!";
                AnnounceWinner();
            }
        }

        private async void AnnounceWinner()
        {
            var winner = _playerScores.OrderByDescending(ps => ps.Value).First();
            await Application.Current.MainPage.DisplayAlert("Game Over",
                $"{winner.Key} wins with {winner.Value} correct answers!",
                "OK");
        }

        private void OnSelectAnswer(string selectedIndicator)
        {
            var selectedOption = Answers.FirstOrDefault(a => a.Index == selectedIndicator);
            if (selectedOption != null)
            {
                SelectedAnswer = selectedOption;
            }
        }

        private void OnSubmitAnswer()
        {
            if (SelectedAnswer == null) return;

            var currentPlayer = _playerNames[_currentPlayerIndex];
            var currentQuestion = _triviaQuestions[_currentQuestionIndex];

            if (SelectedAnswer.Text == currentQuestion.CorrectAnswer)
            {
                _playerScores[currentPlayer]++;
            }

            _currentQuestionIndex++;
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _playerNames.Count;

            if (_currentPlayerIndex == 0)
            {
                _currentRound++;
            }

            DisplayQuestion();
        }

        private void UpdateScoreDisplay()
        {
            ScoreDisplay = string.Join(", ", _playerScores.Select(ps => $"{ps.Key}: {ps.Value}"));
            TurnDisplay = _currentRound < _numberOfRounds
                ? $"{_playerNames[_currentPlayerIndex]}'s turn (Round {_currentRound + 1}/{_numberOfRounds})"
                : "Game finished";
        }

        public bool ValidatePlayerNames()
        {
            foreach (var name in _playerNames)
            {
                if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AnswerOption : BaseViewModel
    {
        public string Index { get; set; }
        public string Text { get; set; }
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
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
