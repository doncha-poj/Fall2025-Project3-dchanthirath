using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    public class Movie
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

        // This stores the image in the database
        public byte[]? Poster { get; set; }

        // Navigation property for the many-to-many relationship
        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}