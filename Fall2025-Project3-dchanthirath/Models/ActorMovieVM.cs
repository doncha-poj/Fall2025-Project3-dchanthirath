using Microsoft.AspNetCore.Mvc.Rendering;
using Fall2025_Project3_dchanthirath.Models;

namespace Fall2025_Project3_dchanthirath.ViewModels
{
    public class ActorMovieVM
    {
        public ActorMovie ActorMovie { get; set; }
        public IEnumerable<SelectListItem>? Actors { get; set; }
        public IEnumerable<SelectListItem>? Movies { get; set; }
    }
}