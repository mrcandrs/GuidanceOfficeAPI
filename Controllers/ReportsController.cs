using GuidanceOfficeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
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
    }
}