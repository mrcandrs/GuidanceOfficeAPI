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
        private readonly TimeZoneInfo _philippinesTimeZone;

        public MoodTrackerController(AppDbContext context)
        {
            _context = context;
            // Initialize Philippines timezone
            _philippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        }

        // Helper method to get current Philippines time
        private DateTime GetPhilippinesNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _philippinesTimeZone);
        }

        // Helper method to get Philippines date (for date comparisons)
        private DateTime GetPhilippinesDate()
        {
            return GetPhilippinesNow().Date;
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
                    EntryDate = DateTime.UtcNow  // Store UTC, not Philippines time
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
        // Returns last 7 days (including today) aggregated by day using Philippines timezone
        [HttpGet("daily-trends")]
        public async Task<IActionResult> GetDailyTrends()
        {
            var philippinesNow = GetPhilippinesNow();
            var startDate = philippinesNow.Date.AddDays(-6); // last 7 days in Philippines time

            var raw = await _context.MoodTrackers
                .Where(m => m.EntryDate >= startDate)
                .ToListAsync(); // load recent records then group in memory

            // Convert stored UTC dates to Philippines timezone for grouping
            var grouped = raw
                .Select(m => new
                {
                    OriginalEntry = m,
                    PhilippinesDate = TimeZoneInfo.ConvertTimeFromUtc(m.EntryDate, _philippinesTimeZone).Date
                })
                .GroupBy(m => m.PhilippinesDate)
                .Select(g => new
                {
                    date = g.Key,
                    mild = g.Count(x => x.OriginalEntry.MoodLevel == "MILD"),
                    moderate = g.Count(x => x.OriginalEntry.MoodLevel == "MODERATE"),
                    high = g.Count(x => x.OriginalEntry.MoodLevel == "HIGH"),
                    na = g.Count(x => string.IsNullOrWhiteSpace(x.OriginalEntry.MoodLevel))
                })
                .OrderBy(x => x.date)
                .ToList();

            // Ensure every day in range has an entry (fill zeroes) using Philippines dates
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
        // Returns simple alert messages using Philippines timezone
        [HttpGet("alerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var philippinesNow = GetPhilippinesNow();
            var oneWeekAgo = philippinesNow.Date.AddDays(-6);

            var recent = await _context.MoodTrackers
                .Where(m => m.EntryDate >= oneWeekAgo)
                .ToListAsync();

            // Filter based on Philippines timezone dates
            var recentInPhilippinesTime = recent
                .Where(m => TimeZoneInfo.ConvertTimeFromUtc(m.EntryDate, _philippinesTimeZone).Date >= oneWeekAgo.Date)
                .ToList();

            var highCount = recentInPhilippinesTime.Count(m => m.MoodLevel == "HIGH");
            var moderateCount = recentInPhilippinesTime.Count(m => m.MoodLevel == "MODERATE");

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
            var highStudents = recentInPhilippinesTime
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

        // GET: api/moodtracker/monthly-reports?month=8&year=2024
        // Uses Philippines timezone for month boundaries
        [HttpGet("monthly-reports")]
        public async Task<IActionResult> GetMonthlyReports([FromQuery] int month, [FromQuery] int year)
        {
            try
            {
                // Validate input parameters
                if (month < 1 || month > 12)
                {
                    return BadRequest("Month must be between 1 and 12");
                }

                var currentPhilippinesYear = GetPhilippinesNow().Year;
                if (year < 2020 || year > currentPhilippinesYear + 1)
                {
                    return BadRequest("Invalid year provided");
                }

                // Create month boundaries in Philippines timezone
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                // Convert to UTC for database query (assuming stored dates are in UTC)
                var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(startDate, _philippinesTimeZone);
                var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(endDate, _philippinesTimeZone);

                // Fetch all mood entries for the specified month
                var monthlyEntries = await _context.MoodTrackers
                    .Where(m => m.EntryDate >= startDateUtc && m.EntryDate < endDateUtc)
                    .ToListAsync();

                // Group by weeks within the month using Philippines timezone
                var weeklyData = new List<object>();
                var currentWeekStart = startDate;
                int weekNumber = 1;

                while (currentWeekStart < endDate)
                {
                    // Calculate the end of the current week (or end of month if sooner)
                    var currentWeekEnd = currentWeekStart.AddDays(7);
                    if (currentWeekEnd > endDate)
                        currentWeekEnd = endDate;

                    // Convert week boundaries to UTC for filtering
                    var currentWeekStartUtc = TimeZoneInfo.ConvertTimeToUtc(currentWeekStart, _philippinesTimeZone);
                    var currentWeekEndUtc = TimeZoneInfo.ConvertTimeToUtc(currentWeekEnd, _philippinesTimeZone);

                    // Filter entries for this week
                    var weekEntries = monthlyEntries
                        .Where(m => m.EntryDate >= currentWeekStartUtc && m.EntryDate < currentWeekEndUtc)
                        .ToList();

                    // Count mood levels for this week
                    var weekData = new
                    {
                        week = $"Week {weekNumber}",
                        weekStart = currentWeekStart.ToString("MMM dd"),
                        weekEnd = currentWeekEnd.AddDays(-1).ToString("MMM dd"),
                        mild = weekEntries.Count(x => x.MoodLevel == "MILD"),
                        moderate = weekEntries.Count(x => x.MoodLevel == "MODERATE"),
                        high = weekEntries.Count(x => x.MoodLevel == "HIGH"),
                        na = weekEntries.Count(x => string.IsNullOrWhiteSpace(x.MoodLevel) || x.MoodLevel == "N/A"),
                        totalEntries = weekEntries.Count
                    };

                    weeklyData.Add(weekData);

                    // Move to next week
                    currentWeekStart = currentWeekEnd;
                    weekNumber++;
                }

                return Ok(weeklyData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}