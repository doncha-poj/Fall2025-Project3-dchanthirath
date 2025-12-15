using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models.ActorViewModels
{
    // Helper class for the Tweet table rows
    public class TweetSentiment
    {
        public string TweetText { get; set; }
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; }
    }

    // ViewModel for the Details page
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }

        // Requirement: Table of 20 tweets and sentiment
        public List<TweetSentiment> Tweets { get; set; } = new List<TweetSentiment>();

        // Requirement: Heading with overall sentiment
        public double OverallSentimentScore { get; set; }
        public string OverallSentimentLabel { get; set; }

        // Requirement: List of movies with the actor
        public List<Movie> MoviesWithActor { get; set; } = new List<Movie>();
    }
}