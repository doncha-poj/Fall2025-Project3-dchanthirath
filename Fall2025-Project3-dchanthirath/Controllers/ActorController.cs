using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_dchanthirath.Data;
using Fall2025_Project3_dchanthirath.Models;
using Fall2025_Project3_dchanthirath.Services;
using VaderSharp2;
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

            // 1. Generate AI Tweets
            // Note: Ensure your OpenAiService has GetActorTweetsAsync implemented (it looked correct in your upload)
            var tweets = await _openAiService.GetActorTweetsAsync(actor.Name);

            // 2. Analyze Sentiment
            var analyzer = new SentimentIntensityAnalyzer();
            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor,
                MoviesWithActor = actor.ActorMovies.Select(am => am.Movie).ToList()
            };

            double totalScore = 0;
            foreach (var tweetText in tweets)
            {
                var sentiment = analyzer.PolarityScores(tweetText);
                totalScore += sentiment.Compound;
                viewModel.Tweets.Add(new TweetSentiment
                {
                    TweetText = tweetText,
                    SentimentScore = sentiment.Compound,
                    SentimentLabel = GetSentimentLabel(sentiment.Compound)
                });
            }

            // 3. Calculate Average Sentiment
            viewModel.OverallSentimentScore = tweets.Any() ? totalScore / tweets.Count : 0;
            viewModel.OverallSentimentLabel = GetSentimentLabel(viewModel.OverallSentimentScore);

            return View(viewModel);
        }

        private string GetSentimentLabel(double score)
        {
            if (score > 0.05) return "Positive";
            if (score < -0.05) return "Negative";
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
                // Handle file upload
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
                    // Fetch existing actor to preserve photo if not replaced
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
                _context.Actors.Remove(actor);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}