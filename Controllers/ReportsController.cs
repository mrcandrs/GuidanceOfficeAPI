using GuidanceOfficeAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize] // Requires authentication
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public ReportsController(AppDbContext ctx) { _ctx = ctx; }

        [HttpGet("appointments")]
        public async Task<IActionResult> Appointments([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.GuidanceAppointments.AsQueryable();
            if (from.HasValue) q = q.Where(a => (a.UpdatedAt ?? a.CreatedAt) >= from.Value);
            if (to.HasValue) q = q.Where(a => (a.UpdatedAt ?? a.CreatedAt) <= to.Value);

            var total = await q.CountAsync();
            var pending = await q.CountAsync(a => a.Status.ToLower() == "pending");
            var approved = await q.CountAsync(a => a.Status.ToLower() == "approved");
            var rejected = await q.CountAsync(a => a.Status.ToLower() == "rejected");
            var completed = await q.CountAsync(a => a.Status.ToLower() == "completed");

            var byDay = await q
                .GroupBy(a => (a.UpdatedAt ?? a.CreatedAt).Date)
                .Select(g => new { date = g.Key, count = g.Count() })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(new { total, pending, approved, rejected, completed, byDay });
        }

        [HttpGet("referrals")]
        public async Task<IActionResult> Referrals([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.ReferralForms.AsQueryable();
            if (from.HasValue) q = q.Where(r => r.SubmissionDate >= from.Value);
            if (to.HasValue) q = q.Where(r => r.SubmissionDate <= to.Value);

            var total = await q.CountAsync();
            var byPriority = await q
                .GroupBy(r => r.PriorityLevel ?? "Unknown")
                .Select(g => new { priority = g.Key, count = g.Count() })
                .ToListAsync();

            var byCategory = await q
                .GroupBy(r => r.AreasOfConcern ?? "Unknown")
                .Select(g => new { category = g.Key, count = g.Count() })
                .ToListAsync();

            return Ok(new { total, byPriority, byCategory });
        }

        [HttpGet("notes")]
        public async Task<IActionResult> Notes([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.GuidanceNotes.AsQueryable();
            if (from.HasValue) q = q.Where(n => n.InterviewDate >= from.Value);
            if (to.HasValue) q = q.Where(n => n.InterviewDate <= to.Value);

            var total = await q.CountAsync();

            // Group by counseling nature
            var byNature = new List<object>
            {
                new { type = "Academic", count = await q.CountAsync(n => n.IsAcademic) },
                new { type = "Behavioral", count = await q.CountAsync(n => n.IsBehavioral) },
                new { type = "Personal", count = await q.CountAsync(n => n.IsPersonal) },
                new { type = "Social", count = await q.CountAsync(n => n.IsSocial) },
                new { type = "Career", count = await q.CountAsync(n => n.IsCareer) }
            };

            return Ok(new { total, byNature });
        }

        [HttpGet("forms-completion")]
        public async Task<IActionResult> FormsCompletion([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var totalStudents = await _ctx.Students.CountAsync();

            var consentForms = await _ctx.ConsentForms.CountAsync();
            var inventoryForms = await _ctx.InventoryForms.CountAsync();
            var careerForms = await _ctx.CareerPlanningForms.CountAsync();

            return Ok(new
            {
                totalStudents,
                consentForms,
                inventoryForms,
                careerForms,
                consentCompletionRate = totalStudents > 0 ? (double)consentForms / totalStudents * 100 : 0,
                inventoryCompletionRate = totalStudents > 0 ? (double)inventoryForms / totalStudents * 100 : 0,
                careerCompletionRate = totalStudents > 0 ? (double)careerForms / totalStudents * 100 : 0
            });
        }
    }
}
