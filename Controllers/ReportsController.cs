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

            // Group by status (you might need to add a Status field to ConsultationForm model)
            // For now, we'll group by date ranges
            var byMonth = await q
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new {
                    month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    count = g.Count()
                })
                .OrderBy(x => x.month)
                .ToListAsync();

            // Group by counselor
            var byCounselor = await q
                .Include(c => c.Counselor)
                .GroupBy(c => c.Counselor.Name ?? "Unknown")
                .Select(g => new { counselor = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            // Group by grade level
            var byGradeLevel = await q
                .GroupBy(c => c.GradeYearLevel ?? "Unknown")
                .Select(g => new { gradeLevel = g.Key, count = g.Count() })
                .OrderBy(x => x.gradeLevel)
                .ToListAsync();

            return Ok(new
            {
                total,
                byMonth,
                byCounselor,
                byGradeLevel
            });
        }

        [HttpGet("endorsements")]
        public async Task<IActionResult> Endorsements([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.EndorsementCustodyForms.AsQueryable();
            if (from.HasValue) q = q.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(e => e.CreatedAt <= to.Value);

            var total = await q.CountAsync();

            // Group by month
            var byMonth = await q
                .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
                .Select(g => new {
                    month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    count = g.Count()
                })
                .OrderBy(x => x.month)
                .ToListAsync();

            // Group by counselor
            var byCounselor = await q
                .Include(e => e.Counselor)
                .GroupBy(e => e.Counselor.Name ?? "Unknown")
                .Select(g => new { counselor = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            // Group by endorsed to
            var byEndorsedTo = await q
                .GroupBy(e => e.EndorsedTo ?? "Unknown")
                .Select(g => new { endorsedTo = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Ok(new
            {
                total,
                byMonth,
                byCounselor,
                byEndorsedTo
            });
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

            // Group by month
            var byMonth = await q
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new {
                    month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    count = g.Count()
                })
                .OrderBy(x => x.month)
                .ToListAsync();

            // Group by time slots (morning, afternoon, evening)
            var allSlots = await q.ToListAsync();
            var byTimeSlot = allSlots
                .Select(t => new {
                    time = t.Time,
                    isMorning = t.Time.Contains("AM") || (t.Time.Contains(":") && int.Parse(t.Time.Split(':')[0]) < 12),
                    isAfternoon = t.Time.Contains("PM") && (t.Time.Contains(":") && int.Parse(t.Time.Split(':')[0]) >= 12 && int.Parse(t.Time.Split(':')[0]) < 6),
                    isEvening = t.Time.Contains("PM") && (t.Time.Contains(":") && int.Parse(t.Time.Split(':')[0]) >= 6)
                })
                .GroupBy(t =>
                    t.isMorning ? "Morning" :
                    t.isAfternoon ? "Afternoon" :
                    t.isEvening ? "Evening" : "Unknown")
                .Select(g => new { timeSlot = g.Key, count = g.Count() })
                .ToList();

            // Average appointments per slot
            var avgAppointmentsPerSlot = allSlots.Any() ? allSlots.Average(t => t.CurrentAppointmentCount) : 0;

            return Ok(new
            {
                total,
                active,
                inactive,
                byMonth,
                byTimeSlot,
                avgAppointmentsPerSlot = Math.Round(avgAppointmentsPerSlot, 2)
            });
        }

        [HttpGet("guidancepasses")]
        public async Task<IActionResult> GuidancePasses([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var q = _ctx.GuidancePasses.AsQueryable();
            if (from.HasValue) q = q.Where(g => g.IssuedDate >= from.Value);
            if (to.HasValue) q = q.Where(g => g.IssuedDate <= to.Value);

            var total = await q.CountAsync();

            // Group by month
            var byMonth = await q
                .GroupBy(g => new { g.IssuedDate.Year, g.IssuedDate.Month })
                .Select(g => new {
                    month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    count = g.Count()
                })
                .OrderBy(x => x.month)
                .ToListAsync();

            // Group by counselor
            var byCounselor = await q
                .Include(g => g.Counselor)
                .GroupBy(g => g.Counselor.Name ?? "Unknown")
                .Select(g => new { counselor = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            // Group by appointment status (through the related appointment)
            var byAppointmentStatus = await q
                .Include(g => g.Appointment)
                .GroupBy(g => g.Appointment.Status ?? "Unknown")
                .Select(g => new { status = g.Key, count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                total,
                byMonth,
                byCounselor,
                byAppointmentStatus
            });
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
