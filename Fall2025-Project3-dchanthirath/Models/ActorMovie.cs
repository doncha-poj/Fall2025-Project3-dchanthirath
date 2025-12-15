namespace Fall2025_Project3_dchanthirath.Models
{
    // This is the join table for the many-to-many relationship
    // between Actors and Movies.
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