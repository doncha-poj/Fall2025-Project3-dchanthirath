using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    // Helper class for the Tweet sentiment
    public class TweetSentiment
    {
        public string TweetText { get; set; }
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; }
    }

    // ViewModel for the Actor Details page
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<TweetSentiment> Tweets { get; set; } = new List<TweetSentiment>();
        public double OverallSentimentScore { get; set; }
        public string OverallSentimentLabel { get; set; }
        public List<Movie> MoviesWithActor { get; set; } = new List<Movie>();
    }
}