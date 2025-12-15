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

        [Range(0, 120)]
        public int Age { get; set; }

        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbHyperlink { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime BirthDate { get; set; }

        // This stores the image in the database
        public byte[]? Photo { get; set; }

        // Navigation property for the many-to-many relationship
        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}