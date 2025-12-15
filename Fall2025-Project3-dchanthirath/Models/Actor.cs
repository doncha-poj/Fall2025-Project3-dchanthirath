using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Gender { get; set; }

        // Requirement: Age attribute
        [Range(0, 120)]
        public int Age { get; set; }

        // Requirement: IMDB hyperlink
        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbHyperlink { get; set; }

        // Requirement: Photo stored as byte[]
        [Display(Name = "Actor Photo")]
        public byte[]? Photo { get; set; }

        // Navigation property for related movies
        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}