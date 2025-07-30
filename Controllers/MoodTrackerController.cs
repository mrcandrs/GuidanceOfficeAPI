using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;
using GuidanceOfficeAPI.Dtos;

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

    }

}
