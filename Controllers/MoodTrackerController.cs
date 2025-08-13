using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using GuidanceOfficeAPI.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodTrackerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MoodTrackerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostMoodEntry([FromBody] MoodTrackerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var mood = new MoodTracker
                {
                    StudentId = dto.StudentId,
                    MoodLevel = dto.MoodLevel,
                    EntryDate = DateTime.Now
                };

                _context.MoodTrackers.Add(mood);
                await _context.SaveChangesAsync();

                return Ok(mood);
            }
            catch (Exception ex)
            {
                // Return the exception message for debugging (remove this in production!)
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // GET: api/moodtracker/distribution
        [HttpGet("distribution")]
        public async Task<IActionResult> GetMoodDistribution()
        {
            var distribution = await _context.MoodTrackers
                .GroupBy(m => m.MoodLevel)
                .Select(g => new
                {
                    mood = g.Key ?? "N/A",
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(distribution);
        }

        // GET: api/moodtracker/daily-trends
        //Returns last 7 days (including today) aggregated by day
        [HttpGet("daily-trends")]
        public async Task<IActionResult> GetDailyTrends()
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-6); //last 7 days
            var raw = await _context.MoodTrackers
                .Where(m => m.EntryDate >= startDate)
                .ToListAsync(); //load recent records then group in memory (safe for small/medium datasets)

            var grouped = raw
                .GroupBy(m => m.EntryDate.Date)
                .Select(g => new
                {
                    date = g.Key,
                    mild = g.Count(x => x.MoodLevel == "MILD"),
                    moderate = g.Count(x => x.MoodLevel == "MODERATE"),
                    high = g.Count(x => x.MoodLevel == "HIGH"),
                    na = g.Count(x => string.IsNullOrWhiteSpace(x.MoodLevel))
                })
                .OrderBy(x => x.date)
                .ToList();

            // Ensure every day in range has an entry (fill zeroes)
            var result = Enumerable.Range(0, 7).Select(i =>
            {
                var day = startDate.AddDays(i);
                var found = grouped.FirstOrDefault(g => g.date == day);
                return new
                {
                    date = day.ToString("MMM dd"),
                    mild = found?.mild ?? 0,
                    moderate = found?.moderate ?? 0,
                    high = found?.high ?? 0,
                    na = found?.na ?? 0
                };
            }).ToList();

            return Ok(result);
        }

        // GET: api/moodtracker/alerts
        //Returns simple alert messages (example rule based)
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var oneWeekAgo = DateTime.UtcNow.Date.AddDays(-6);
            var recent = await _context.MoodTrackers
                .Where(m => m.EntryDate >= oneWeekAgo)
                .ToListAsync();

            var highCount = recent.Count(m => m.MoodLevel == "HIGH");
            var moderateCount = recent.Count(m => m.MoodLevel == "MODERATE");

            var alerts = new System.Collections.Generic.List<object>();

            if (highCount >= 5) // arbitrary threshold — tune as needed
            {
                alerts.Add(new { level = "high", message = $"{highCount} HIGH mood reports in the last 7 days" });
            }

            if (moderateCount >= 8)
            {
                alerts.Add(new { level = "moderate", message = $"{moderateCount} MODERATE mood reports in the last 7 days" });
            }

            // example: list students who reported HIGH in last 7 days (limit 10)
            var highStudents = recent
                .Where(m => m.MoodLevel == "HIGH")
                .Select(m => m.StudentId)
                .Distinct()
                .Take(10)
                .ToList();

            if (highStudents.Any())
            {
                alerts.Add(new { level = "info", message = $"Students with recent HIGH mood entries: {string.Join(", ", highStudents)}" });
            }

            return Ok(alerts);
        }

    }

}
