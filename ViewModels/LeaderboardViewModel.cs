using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Maui.Storage; // Not sure whats going on here? it errors without but is not being used
using System.Linq;
using System;

namespace CrossPlatformProject2.ViewModels
{
    public class LeaderboardViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ScoreEntry> _scores;
        public ObservableCollection<ScoreEntry> Scores
        {
            get => _scores;
            set
            {
                if (_scores != value)
                {
                    _scores = value;
                    OnPropertyChanged();
                }
            }
        }

        private static readonly string LeaderboardFilePath = Path.Combine(FileSystem.AppDataDirectory, "LeaderboardData.json");

        public LeaderboardViewModel(Dictionary<string, int> playerScores)
        {
            // Load saved scores from file if they exist
            Scores = LoadScoresFromFile() ?? new ObservableCollection<ScoreEntry>();

            // If new scores are passed, update the leaderboard
            if (playerScores != null)
            {
                foreach (var score in playerScores)
                {
                    AddOrUpdateScore(score.Key, score.Value);
                }

                // Sort the scores in descending order after updating
                var sortedScores = Scores.OrderByDescending(s => s.score).ToList();
                Scores = new ObservableCollection<ScoreEntry>(sortedScores);

                // Save the updated leaderboard to the file
                SaveScoresToFile();
            }
        }

        public void AddOrUpdateScore(string playerName, int score)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));
            }

            if (score < 0)
            {
                throw new ArgumentException("Score cannot be negative.", nameof(score));
            }

            var existingEntry = Scores.FirstOrDefault(s => s.playerName == playerName);
            if (existingEntry != null)
            {
                existingEntry.score = Math.Max(existingEntry.score, score);
            }
            else
            {
                Scores.Add(new ScoreEntry { playerName = playerName, score = score });
            }
            SaveScoresToFile();
        }

        public void ResetLeaderboard()
        {
            if (Scores != null)
            {
                Scores.Clear();
                SaveScoresToFile();
                OnPropertyChanged(nameof(Scores));
            }
            else
            {
                // Initialize Scores if it's somehow null
                Scores = new ObservableCollection<ScoreEntry>();
                SaveScoresToFile();
                OnPropertyChanged(nameof(Scores));
            }
        }

        private void SaveScoresToFile()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Scores);
                File.WriteAllText(LeaderboardFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save leaderboard: {ex.Message}");
                // Optionally, notify the user about the failure
            }
        }

        private ObservableCollection<ScoreEntry> LoadScoresFromFile()
        {
            try
            {
                if (File.Exists(LeaderboardFilePath))
                {
                    var json = File.ReadAllText(LeaderboardFilePath);
                    var loadedScores = JsonConvert.DeserializeObject<ObservableCollection<ScoreEntry>>(json);
                    return loadedScores ?? new ObservableCollection<ScoreEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load leaderboard: {ex.Message}");
                // Optionally, notify the user about the failure
            }
            return new ObservableCollection<ScoreEntry>(); // Return an empty collection instead of null
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class ScoreEntry // Class represents data entered for a score 
        {
            public string playerName { get; set; }
            public int score { get; set; }
        }
    }
}
