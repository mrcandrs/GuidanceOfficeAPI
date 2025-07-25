using Microsoft.AspNetCore.Mvc;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using GuidanceOfficeAPI.Models;

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

            return Ok(new { message = "Journal entry saved successfully!" });
        }
    }
}
