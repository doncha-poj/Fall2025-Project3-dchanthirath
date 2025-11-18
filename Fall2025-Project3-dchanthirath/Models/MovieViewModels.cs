using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    // A helper class for the Details page
    public class ReviewSentiment
    {
        public string ReviewText { get; set; }
        public double SentimentScore { get; set; }
        public string SentimentLabel { get; set; }
    }

    // ViewModel for the Movie Details page
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<ReviewSentiment> Reviews { get; set; } = new List<ReviewSentiment>();
        public double OverallSentimentScore { get; set; }
        public string OverallSentimentLabel { get; set; }
        public List<Actor> ActorsInMovie { get; set; } = new List<Actor>();
    }

    // ViewModel for the Movie Create page
    public class MovieCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbHyperlink { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Range(1888, 2030)]
        public int Year { get; set; }

        [Display(Name = "Movie Poster")]
        public IFormFile? Poster { get; set; }
    }

    // ViewModel for the Movie Edit page
    public class MovieEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbHyperlink { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Range(1888, 2030)]
        public int Year { get; set; }

        [Display(Name = "New Movie Poster")]
        public IFormFile? NewPoster { get; set; }

        // To display the existing poster
        public byte[]? ExistingPoster { get; set; }
    }
}