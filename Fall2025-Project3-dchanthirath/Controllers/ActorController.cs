using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.Models.ActorViewModels; // Ensure this matches your ViewModel namespace
using Fall2025_Project3_dchanthirath.Services;
using VaderSharp2; // Requirement: Sentiment Analysis
using Microsoft.AspNetCore.Authorization;

namespace Fall2025_Project3_dchanthirath.Controllers
{
    [Authorize]
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAiService _openAiService;

        public ActorsController(ApplicationDbContext context, OpenAiService openAiService)
        {
            _context = context;
            _openAiService = openAiService;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null) return NotFound();

            // --- Requirement: Call AI and Sentiment APIs ---

            // 1. Get 20 AI Tweets (One API Call)
            // Note: Ensure your OpenAiService.GetActorTweetsAsync is set to request 20 tweets in its prompt
            var tweets = await _openAiService.GetActorTweetsAsync(actor.Name);

            // 2. Analyze Sentiment using VaderSharp2
            var analyzer = new SentimentIntensityAnalyzer();
            var tweetSentiments = new List<TweetSentiment>();
            double totalScore = 0;

            foreach (var tweet in tweets)
            {
                var sentimentResults = analyzer.PolarityScores(tweet);
                var score = sentimentResults.Compound;

                totalScore += score;

                tweetSentiments.Add(new TweetSentiment
                {
                    TweetText = tweet,
                    SentimentScore = score,
                    SentimentLabel = GetSentimentLabel(score)
                });
            }

            // 3. Calculate Average
            double averageScore = tweets.Any() ? totalScore / tweets.Count : 0;

            // 4. Prepare ViewModel
            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor,
                MoviesWithActor = actor.ActorMovies.Select(am => am.Movie).ToList(), // Requirement: List of movies
                Tweets = tweetSentiments, // Requirement: Table data
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

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ImdbHyperlink")] Actor actor, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload (Requirement: Store as byte[])
                if (Photo != null && Photo.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Photo.CopyToAsync(memoryStream);
                        actor.Photo = memoryStream.ToArray();
                    }
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbHyperlink")] Actor actor, IFormFile? NewPhoto)
        {
            if (id != actor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Need to fetch existing actor to keep the photo if it wasn't changed
                    var existingActor = await _context.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (NewPhoto != null && NewPhoto.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await NewPhoto.CopyToAsync(memoryStream);
                            actor.Photo = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        // Keep old photo
                        actor.Photo = existingActor?.Photo;
                    }

                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Actors.Any(e => e.Id == actor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                // Explicitly remove relationships if needed (though cascade often handles this)
                var actorMovies = _context.ActorMovies.Where(am => am.ActorId == id);
                _context.ActorMovies.RemoveRange(actorMovies);

                _context.Actors.Remove(actor);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}