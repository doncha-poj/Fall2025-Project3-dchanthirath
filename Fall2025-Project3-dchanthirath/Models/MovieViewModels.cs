using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    // Helper class for the Review table
    public class ReviewSentiment
    {
        public string ReviewText { get; set; }
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; }
    }

    // ViewModel for the Details page
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }

        // Requirement: Table columns for Reviews and Sentiment
        public List<ReviewSentiment> Reviews { get; set; } = new List<ReviewSentiment>();

        // Requirement: Heading with average overall sentiment
        public double OverallSentimentScore { get; set; }
        public string OverallSentimentLabel { get; set; }

        // Requirement: List of actors in the movie
        public List<Actor> ActorsInMovie { get; set; } = new List<Actor>();
    }

    // ViewModel for Create/Edit (Standard)
    public class MovieCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Url]
        public string ImdbHyperlink { get; set; }

        [Required]
        public string Genre { get; set; }

        [Range(1888, 2030)]
        public int Year { get; set; }

        public IFormFile? Poster { get; set; }
    }

    public class MovieEditViewModel : MovieCreateViewModel
    {
        public int Id { get; set; }
        public byte[]? ExistingPoster { get; set; }
        public IFormFile? NewPoster { get; set; }
    }
}