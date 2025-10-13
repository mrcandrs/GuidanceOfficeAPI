using Microsoft.AspNetCore.Mvc;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/journalentry")]
    public class JournalEntryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JournalEntryController(AppDbContext context)
        {
            _context = context;
        }
        private static string GetTodayPhKey()
        {
            var ph = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ph).Date.ToString("yyyy-MM-dd");
        }

        [HttpGet("today/{studentId:int}")]
        public async Task<IActionResult> GetToday(int studentId)
        {
            var todayKey = GetTodayPhKey();
            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.StudentId == studentId && j.Date == todayKey);
            return Ok(entry); // null if none
        }

        //POST: api/journalentry/submit-entry
        [HttpPost("submit-entry")]
        public async Task<IActionResult> SubmitEntry([FromBody] JournalEntryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content is required." });

            var todayKey = GetTodayPhKey();
            var existing = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.StudentId == dto.StudentId && j.Date == todayKey);

            if (existing != null)
                return Conflict(new { message = "Entry already exists for today.", journalId = existing.JournalId });

            var entry = new JournalEntry
            {
                StudentId = dto.StudentId,
                Date = todayKey,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Entry" : dto.Title,
                Content = dto.Content,
                Mood = dto.Mood
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();
            return Ok(entry);
        }

        // GET: api/journalentry/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetJournalEntriesByStudent(int studentId)
        {
            var entries = await _context.JournalEntries
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(entries);
        }


        // PUT: api/journalentry/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEntry(int id, [FromBody] JournalEntryDto dto)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry == null) return NotFound(new { message = "Journal entry not found." });

            entry.Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Entry" : dto.Title;
            entry.Content = dto.Content;
            entry.Mood = dto.Mood;
            // keep entry.Date as-is to avoid breaking the one-per-day rule

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Journal entry updated successfully!" });
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("UNIQUE") == true)
            {
                return Conflict(new { message = "A journal entry for that date already exists." });
            }
        }

        [HttpPut("update-today/{studentId:int}")]
        public async Task<IActionResult> UpdateToday(int studentId, [FromBody] JournalEntryDto dto)
        {
            var todayKey = GetTodayPhKey();
            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(j => j.StudentId == studentId && j.Date == todayKey);

            if (entry == null)
                return NotFound(new { message = "No entry for today to update." });

            entry.Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Entry" : dto.Title;
            entry.Content = dto.Content;
            entry.Mood = dto.Mood;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Updated.", journalId = entry.JournalId });
        }

        // DELETE: api/journalentry/delete/{id}
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteEntry(int id)
        {
            var entry = _context.JournalEntries.FirstOrDefault(j => j.JournalId == id);
            if (entry == null)
            {
                return NotFound(new { message = "Journal entry not found." });
            }

            _context.JournalEntries.Remove(entry);
            _context.SaveChanges();

            return Ok(new { message = "Journal entry deleted successfully." });
        }

    }
}
