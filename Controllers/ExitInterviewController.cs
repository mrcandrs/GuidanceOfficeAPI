using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/exitinterview")]
    public class ExitInterviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExitInterviewController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] ExitInterviewForm model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_context.Students.Any(s => s.StudentId == model.StudentId))
                return BadRequest("Student not found.");

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            model.SubmittedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            _context.ExitInterviewForms.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exit Interview Form submitted successfully." });
        }

    }

}
