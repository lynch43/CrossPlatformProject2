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
        private const string ApiUrl = "https://opentdb.com/api.php?amount=10&category=22&difficulty=medium&type=multiple";

        private ObservableCollection<AnswerOption> _answers = new();
        private string _currentQuestion;
        private AnswerOption _selectedAnswer;
        private string _scoreDisplay;
        private string _turnDisplay;
        private int _currentQuestionIndex;
        private int _currentPlayerIndex;
        private List<TriviaQuestion> _triviaQuestions = new();
        private Dictionary<string, int> _playerScores = new();
        private List<string> _playerNames = new();

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

        public GamePageViewModel()
        {
            SubmitAnswerCommand = new Command(OnSubmitAnswer);
            SelectAnswerCommand = new Command<string>(OnSelectAnswer);

            _playerNames = new List<string> { "Player 1", "Player 2" };
            foreach (var name in _playerNames)
            {
                _playerScores[name] = 0;
            }
            FetchTriviaQuestions();
        }

        public GamePageViewModel(List<string> playerNames) : this()
        {
            _playerNames = playerNames;
            _playerScores.Clear();
            foreach (var name in playerNames)
            {
                _playerScores[name] = 0;
            }
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

                if (triviaData?.Results != null)
                {
                    _triviaQuestions = triviaData.Results;
                    DisplayQuestion();
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private void DisplayQuestion()
        {
            if (_currentQuestionIndex < _triviaQuestions.Count)
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
                CurrentQuestion = "Quiz Complete!";
                TurnDisplay = "Quiz finished";
            }
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

            if (_currentQuestionIndex < _triviaQuestions.Count)
            {
                DisplayQuestion();
            }
            else
            {
                UpdateScoreDisplay();
            }
        }

        private void UpdateScoreDisplay()
        {
            ScoreDisplay = string.Join(", ", _playerScores.Select(ps => $"{ps.Key}: {ps.Value}"));
            TurnDisplay = _currentQuestionIndex < _triviaQuestions.Count
                ? $"{_playerNames[_currentPlayerIndex]}'s turn"
                : "Quiz finished";
        }
    }

    public class AnswerOption : BaseViewModel
    {
        public string Index { get; set; } // A, B, C, D
        public string Text { get; set; } // The answer text

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
