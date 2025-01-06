using Newtonsoft.Json;

namespace CrossPlatformProject2.Models
{
    public class CategoryRoot
    {
        [JsonProperty("trivia_categories")]
        public List<TriviaCategory>? TriviaCategories { get; set; } = new List<TriviaCategory>();
    }
}

