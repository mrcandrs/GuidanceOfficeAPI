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

        //POST: api/journalentry/submit-entry
        [HttpPost("submit-entry")]
        public IActionResult SubmitEntry([FromBody] JournalEntryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Content is required." });

            var entry = new JournalEntry
            {
                StudentId = dto.StudentId,
                Date = dto.Date,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Entry" : dto.Title,
                Content = dto.Content,
                Mood = dto.Mood
            };

            _context.JournalEntries.Add(entry);
            _context.SaveChanges();

            return Ok(entry); // ✅ This returns the new journalId
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
        public IActionResult UpdateEntry(int id, [FromBody] JournalEntryDto dto)
        {
            var entry = _context.JournalEntries.FirstOrDefault(j => j.JournalId == id);
            if (entry == null)
            {
                return NotFound(new { message = "Journal entry not found." });
            }

            // Update fields
            entry.Title = string.IsNullOrWhiteSpace(dto.Title) ? "Untitled Entry" : dto.Title;
            entry.Content = dto.Content;
            entry.Mood = dto.Mood;
            entry.Date = dto.Date;

            _context.SaveChanges();

            return Ok(new { message = "Journal entry updated successfully!" });
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
