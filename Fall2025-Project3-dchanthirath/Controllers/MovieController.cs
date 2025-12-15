using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.Services;
using VaderSharp2; // Requirement: Sentiment Analysis
using Microsoft.AspNetCore.Authorization;

namespace Fall2025_Project3_dchanthirath.Controllers
{
    [Authorize]
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

            // --- Requirement: Call AI and Sentiment Analysis APIs ---

            // 1. Get AI Reviews (One API Call)
            var reviews = await _openAiService.GetMovieReviewsAsync(movie.Title);

            // 2. Analyze Sentiment using VaderSharp2
            var analyzer = new SentimentIntensityAnalyzer();
            var reviewSentiments = new List<ReviewSentiment>();
            double totalScore = 0;

            foreach (var review in reviews)
            {
                var sentimentResults = analyzer.PolarityScores(review);
                var score = sentimentResults.Compound;

                totalScore += score;

                reviewSentiments.Add(new ReviewSentiment
                {
                    ReviewText = review,
                    SentimentScore = score,
                    SentimentLabel = GetSentimentLabel(score)
                });
            }

            // 3. Calculate Average
            double averageScore = reviews.Any() ? totalScore / reviews.Count : 0;

            // 4. Prepare ViewModel
            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie,
                ActorsInMovie = movie.ActorMovies.Select(am => am.Actor).ToList(), // Requirement: List of actors
                Reviews = reviewSentiments, // Requirement: Table data
                OverallSentimentScore = averageScore,
                OverallSentimentLabel = GetSentimentLabel(averageScore) // Requirement: Heading data
            };

            return View(viewModel);
        }

        private string GetSentimentLabel(double score)
        {
            if (score >= 0.05) return "Positive";
            if (score <= -0.05) return "Negative";
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

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
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
                // Clean up relationships first if necessary, 
                // though Cascade Delete usually handles this in many setups.
                // Explicitly including related data to be safe:
                var actorMovies = _context.ActorMovies.Where(am => am.MovieId == id);
                _context.ActorMovies.RemoveRange(actorMovies);

                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}