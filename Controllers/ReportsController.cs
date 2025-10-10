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

        [HttpGet("consultations")]
        public async Task<IActionResult> Consultations([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.ConsultationForms.AsQueryable();
            if (from.HasValue) q = q.Where(c => c.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(c => c.CreatedAt <= to.Value);

            var total = await q.CountAsync();

            // Create byStatus array - since ConsultationForm doesn't have a Status field,
            // we'll create a simple status based on whether it has remarks or not
            var byStatus = new List<object>
            {
                new { status = "Completed", count = await q.CountAsync(c => !string.IsNullOrEmpty(c.Remarks)) },
                new { status = "In Progress", count = await q.CountAsync(c => string.IsNullOrEmpty(c.Remarks)) }
            };

            return Ok(new { total, byStatus });
        }

        [HttpGet("endorsements")]
        public async Task<IActionResult> Endorsements([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.EndorsementCustodyForms.AsQueryable();
            if (from.HasValue) q = q.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(e => e.CreatedAt <= to.Value);

            var total = await q.CountAsync();

            // Group by endorsed to (treating this as "type")
            var byType = await q
                .GroupBy(e => e.EndorsedTo ?? "Unknown")
                .Select(g => new { type = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(new { total, byType });
        }

        [HttpGet("timeslots")]
        public async Task<IActionResult> TimeSlots([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.AvailableTimeSlots.AsQueryable();
            if (from.HasValue) q = q.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(t => t.CreatedAt <= to.Value);

            var total = await q.CountAsync();
            var active = await q.CountAsync(t => t.IsActive);
            var inactive = await q.CountAsync(t => !t.IsActive);

            // Create byStatus array
            var byStatus = new List<object>
            {
                new { status = "Active", count = active },
                new { status = "Inactive", count = inactive }
            };

            return Ok(new { total, byStatus });
        }

        [HttpGet("guidancepasses")]
        public async Task<IActionResult> GuidancePasses([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.GuidancePasses.AsQueryable();
            if (from.HasValue) q = q.Where(g => g.IssuedDate >= from.Value);
            if (to.HasValue) q = q.Where(g => g.IssuedDate <= to.Value);

            var total = await q.CountAsync();

            // Group by appointment status (through the related appointment)
            var byStatus = await q
                .Include(g => g.Appointment)
                .GroupBy(g => g.Appointment.Status ?? "Unknown")
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync();

            return Ok(new { total, byStatus });
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
