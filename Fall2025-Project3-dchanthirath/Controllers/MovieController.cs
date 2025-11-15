using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.ViewModels;
using Fall2025_Project3_dchanthirath.Services;
using VaderSharp2;
using Microsoft.AspNetCore.Authorization;

namespace Fall2025_Project3_dchanthirath.Controllers
{
    [Authorize] // Require users to be logged in for all movie actions
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAiService _openAiService;

        public MoviesController(ApplicationDbContext context, OpenAiService openAiService)
        {
            _context = context;
            _openAiService = openAiService;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            // 1. Generate AI Reviews
            var reviews = await _openAiService.GetMovieReviewsAsync(movie.Title);

            // 2. Analyze Sentiment
            var analyzer = new SentimentIntensityAnalyzer();
            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                ActorsInMovie = movie.ActorMovies.Select(am => am.Actor).ToList()
            };

            double totalScore = 0;
            foreach (var reviewText in reviews)
            {
                var sentiment = analyzer.PolarityScores(reviewText);
                totalScore += sentiment.Compound;
                viewModel.Reviews.Add(new ReviewSentiment
                {
                    ReviewText = reviewText,
                    SentimentScore = sentiment.Compound,
                    SentimentLabel = GetSentimentLabel(sentiment.Compound)
                });
            }

            // 3. Calculate Average Sentiment
            viewModel.OverallSentimentScore = reviews.Any() ? totalScore / reviews.Count : 0;
            viewModel.OverallSentimentLabel = GetSentimentLabel(viewModel.OverallSentimentScore);

            return View(viewModel);
        }

        private string GetSentimentLabel(double score)
        {
            if (score > 0.05) return "Positive";
            if (score < -0.05) return "Negative";
            return "Neutral";
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieCreateViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Title = vm.Title,
                    ImdbHyperlink = vm.ImdbHyperlink,
                    Genre = vm.Genre,
                    Year = vm.Year
                };

                // Handle file upload
                if (vm.Poster != null && vm.Poster.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await vm.Poster.CopyToAsync(memoryStream);
                        movie.Poster = memoryStream.ToArray();
                    }
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var vm = new MovieEditViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                ImdbHyperlink = movie.ImdbHyperlink,
                Genre = movie.Genre,
                Year = movie.Year,
                ExistingPoster = movie.Poster
            };

            return View(vm);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var movieToUpdate = await _context.Movies.FindAsync(id);
                    if (movieToUpdate == null) return NotFound();

                    movieToUpdate.Title = vm.Title;
                    movieToUpdate.ImdbHyperlink = vm.ImdbHyperlink;
                    movieToUpdate.Genre = vm.Genre;
                    movieToUpdate.Year = vm.Year;

                    // Handle new file upload
                    if (vm.NewPoster != null && vm.NewPoster.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await vm.NewPoster.CopyToAsync(memoryStream);
                            movieToUpdate.Poster = memoryStream.ToArray();
                        }
                    }

                    _context.Update(movieToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Movies.Any(e => e.Id == vm.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                // This will also remove related ActorMovie entries
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}