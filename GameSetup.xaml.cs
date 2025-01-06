using Newtonsoft.Json;
using CrossPlatformProject2.Models; //implement models to gamepage  

namespace CrossPlatformProject2;

public partial class GameSetup : ContentPage
{
    private List<Entry> playerNameEntriesList = new List<Entry>();//list to store player names

    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "SavedGame.json");//file path
    private Dictionary<string, int> Categories = new();


    //dictionary to map category names to IDs

    public GameSetup()
    {
        InitializeComponent();
        //add options for player selection
        playerPicker.Items.Add("1 Player");
        playerPicker.Items.Add("2 Player");
        playerPicker.Items.Add("3 Player");
        playerPicker.Items.Add("4 Player");

        //add difficulty options
        difficultyPicker.Items.Add("Easy");
        difficultyPicker.Items.Add("Medium");
        difficultyPicker.Items.Add("Hard");

        //Question Picker 
        totalQuestionsPicker.Items.Add("1");
        totalQuestionsPicker.Items.Add("2");
        totalQuestionsPicker.Items.Add("3");
        totalQuestionsPicker.Items.Add("4");
        totalQuestionsPicker.Items.Add("5");

        //Default Question Picker Index
        //totalQuestionsPicker.SelectedIndex = 3;


        //populate category picker
        LoadCategories();
    }


    private async void LoadCategories()
    {
        string apiUrl = "https://opentdb.com/api_category.php";

        using HttpClient client = new HttpClient();
        try
        {
            var response = await client.GetStringAsync(apiUrl);

            //deserialize JSON into a dictionary
            var categoryResponse = JsonConvert.DeserializeObject<CategoryRoot>(response);

            //validate category response and populate picker
            if (categoryResponse != null && categoryResponse.TriviaCategories != null)
            {
                foreach (var category in categoryResponse.TriviaCategories)
                {
                    categoryPicker.Items.Add(category.name); //add category to picker
                    Categories[category.name] = category.id; //map category name to ID
                }
            }
            else
            {
                await DisplayAlert("Error", "Failed to load categories from the API.", "OK");
            }
        }
        catch (Exception ex)
        {
            //handle exceptions during category loading
            await DisplayAlert("Error", $"Failed to load categories: {ex.Message}", "OK");
        }
    }

    private void OnPlayerCountChanged(object sender, EventArgs e)
    {
        //clear prvious player names entered
        playerNameEntries.Clear();
        playerNameEntriesList.Clear();

        if (playerPicker.SelectedIndex < 0)
        {
            return;
        }
        //calculate number of players
        int numberOfPlayers = playerPicker.SelectedIndex + 1;

        //for loop to add player names entries based on selection
        for (int i = 1; i <= numberOfPlayers; i++)
        {
            var entry = new Entry
            {
                Placeholder = $"Enter player {i} Name",
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                FontSize = 16,
            };
            //add player entry
            playerNameEntriesList.Add(entry);
            playerNameEntries.Children.Add(entry);
        }

    }

    private async void OnStartButtonClicked_Clicked(object sender, EventArgs e)
    {
        if (playerPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select the number of players.", "OK");
            return;
        }

        if (difficultyPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select the difficulty.", "OK");
            return;
        }

        if (totalQuestionsPicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Please select the total number of questions.", "OK");
            return;
        }

        var playerNames = playerNameEntriesList
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Text))
            .Select(entry => entry.Text)
            .ToList();

        if (playerNames.Count != playerPicker.SelectedIndex + 1)
        {
            await DisplayAlert("Error", "Please fill in all player names.", "OK");
            return;
        }

        string selectedPlayers = playerPicker.SelectedItem.ToString();
        string selectedDifficulty = difficultyPicker.SelectedItem.ToString();
        string selectedCategory = categoryPicker.SelectedItem?.ToString();
        //Question
        string totalQuestions = totalQuestionsPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(selectedCategory) || !Categories.TryGetValue(selectedCategory, out int selectedCategoryId))
        {
            await DisplayAlert("Error", "Invalid category selection.", "OK");
            return;
        }
        if (int.TryParse(totalQuestionsPicker.SelectedItem?.ToString(), out int totalQuestionsParsed))
        {
            await Navigation.PushAsync(new GamePage(selectedPlayers, selectedDifficulty, selectedCategoryId, playerNames, totalQuestionsParsed));

        }
        // Push these values to the GamePage for use
    }

    private async Task LoadGameFromFile()
    {
        if (File.Exists(GamePage.FilePath))
        {
            string json = await File.ReadAllTextAsync(GamePage.FilePath);

            var gameState = JsonConvert.DeserializeObject<GameState>(json);

            if (gameState != null)
            {
                //navigate to the GamePage with the loaded data
                await Navigation.PushAsync(new GamePage(
                    gameState.PlayerNames.Count.ToString() + " Players", // Use number of players
                    gameState.SelectedDifficulty,
                    gameState.selectedCategoryId,
                    gameState.PlayerNames,
                    gameState.TotalQuestions

                //gameState.TriviaQuestions 
                ));
            }
            else
            {
                await DisplayAlert("Error", "Failed to load the saved game.", "OK");
            }
        }
        else
        {
            await DisplayAlert("No Saved Game", "No saved game data found.", "OK");
        }
    }

    private async void OnLoadGameClicked(object sender, EventArgs e)
    {
        await LoadGameFromFile();
    }



    private async void homeButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}