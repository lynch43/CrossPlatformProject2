
namespace CrossPlatformProject2.Models
{
    public class QuestionModel //model to represent each trivia question 
    {
        public string Question { get; set; }//the question
        public string CorrectAnswer { get; set; }//correct answer
        //incorrect answer lsit
        public List<string> IncorrectAnswers { get; set; }
    }
}
