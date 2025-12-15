using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_dchanthirath.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }

        public int ActorId { get; set; }
        public int MovieId { get; set; }

        // Navigation properties
        public virtual Actor? Actor { get; set; }
        public virtual Movie? Movie { get; set; }
    }
}