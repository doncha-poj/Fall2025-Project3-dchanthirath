using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Fall2025_Project3_dchanthirath.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActorMovies
        public async Task<IActionResult> Index()
        {
            // Eager load the related Actor and Movie data
            var actorMovies = _context.ActorMovies
                                      .Include(am => am.Actor)
                                      .Include(am => am.Movie);

            return View(await actorMovies.ToListAsync());
        }

        // GET: ActorMovies/Create
        public IActionResult Create()
        {
            // Use the ViewModel to build the form
            var viewModel = new ActorMovieVM
            {
                ActorMovie = new ActorMovie(),
                Actors = _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList(),
                Movies = _context.Movies.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Title
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: ActorMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorMovieVM viewModel)
        {
            if (ModelState.IsValid)
            {
                // Pull the ActorMovie object from the ViewModel
                _context.Add(viewModel.ActorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If the model is invalid, we must re-populate the dropdowns
            viewModel.Actors = _context.Actors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList();
            viewModel.Movies = _context.Movies.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            }).ToList();

            return View(viewModel); // Return the view with errors and dropdown data
        }

        // ... Add Details, Edit, and Delete actions here ...
    }
}