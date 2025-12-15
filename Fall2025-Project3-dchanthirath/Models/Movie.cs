using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "IMDb Link")]
        [Url] // Ensures it validates as a URL
        public string ImdbHyperlink { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Display(Name = "Year of Release")]
        [Range(1888, 2030)]
        public int Year { get; set; }

        // Requirement: Poster stored as byte[] in db
        [Display(Name = "Poster")]
        public byte[]? Poster { get; set; }

        // Navigation property for the list of actors
        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}