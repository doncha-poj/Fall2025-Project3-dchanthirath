using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Fall2025_Project3_dchanthirath.Controllers
{
    [Authorize] // Require login for all actions in this controller
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActorMovies
        [AllowAnonymous] // Optional: Allow guests to view the list, remove if you want strict privacy
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ActorMovies.Include(a => a.Actor).Include(a => a.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ActorMovies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        // GET: ActorMovies/Create
        public IActionResult Create()
        {
            var viewModel = new ActorMovieVM
            {
                ActorMovie = new ActorMovie(),
                Actors = new SelectList(_context.Actors, "Id", "Name"),
                Movies = new SelectList(_context.Movies, "Id", "Title")
            };
            return View(viewModel);
        }

        // POST: ActorMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorMovieVM viewModel)
        {
            // 1. Check for Duplicates
            bool exists = await _context.ActorMovies.AnyAsync(am =>
                am.ActorId == viewModel.ActorMovie.ActorId &&
                am.MovieId == viewModel.ActorMovie.MovieId);

            if (exists)
            {
                ModelState.AddModelError("", "This actor is already linked to this movie.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(viewModel.ActorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            viewModel.Actors = new SelectList(_context.Actors, "Id", "Name", viewModel.ActorMovie.ActorId);
            viewModel.Movies = new SelectList(_context.Movies, "Id", "Title", viewModel.ActorMovie.MovieId);
            return View(viewModel);
        }

        // GET: ActorMovies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie == null) return NotFound();

            var viewModel = new ActorMovieVM
            {
                ActorMovie = actorMovie,
                Actors = new SelectList(_context.Actors, "Id", "Name", actorMovie.ActorId),
                Movies = new SelectList(_context.Movies, "Id", "Title", actorMovie.MovieId)
            };
            return View(viewModel);
        }

        // POST: ActorMovies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActorMovieVM viewModel)
        {
            if (id != viewModel.ActorMovie.Id) return NotFound();

            // Check for duplicates (excluding the current record)
            bool exists = await _context.ActorMovies.AnyAsync(am =>
                am.Id != id &&
                am.ActorId == viewModel.ActorMovie.ActorId &&
                am.MovieId == viewModel.ActorMovie.MovieId);

            if (exists)
            {
                ModelState.AddModelError("", "This relationship already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewModel.ActorMovie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorMovieExists(viewModel.ActorMovie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.Actors = new SelectList(_context.Actors, "Id", "Name", viewModel.ActorMovie.ActorId);
            viewModel.Movies = new SelectList(_context.Movies, "Id", "Title", viewModel.ActorMovie.MovieId);
            return View(viewModel);
        }

        // GET: ActorMovies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actorMovie = await _context.ActorMovies
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null) return NotFound();

            return View(actorMovie);
        }

        // POST: ActorMovies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovies.FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorMovies.Remove(actorMovie);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorMovieExists(int id)
        {
            return _context.ActorMovies.Any(e => e.Id == id);
        }
    }
}