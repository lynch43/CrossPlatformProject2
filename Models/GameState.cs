namespace CrossPlatformProject2.Models
{
    public class GameState //game state page to save
    {
        public Dictionary<string, int> PlayerScores { get; set; }
        public int CurrentQuestionIndex { get; set; }
        // public List<QuestionModel> RemainingQuestions { get; set; }
        public int CurrentPlayerIndex { get; set; }
        public int TotalQuestions { get; set; }
        public string SelectedDifficulty { get; set; }
        public string SelectedCategory { get; set; }

        public int selectedCategoryId { get; set; }//category id

        public string SelectedPlayers { get; set; }//added to store selected players
        public List<string> PlayerNames { get; set; }

        public List<QuestionModel> TriviaQuestions { get; set; } //added to store question 
    }
}
