using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/history")]
    [Authorize] // Requires authentication
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/history
        [HttpGet]
        public async Task<IActionResult> GetActivityLogs(
            [FromQuery] string? entityType,
            [FromQuery] string? action,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? actorType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            IQueryable<ActivityLog> query = _context.ActivityLogs;

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(x => x.EntityType == entityType);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(x => x.Action == action);

            if (from.HasValue)
                query = query.Where(x => x.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.CreatedAt <= to.Value.AddDays(1));

            if (!string.IsNullOrEmpty(actorType))
                query = query.Where(x => x.ActorType == actorType);

            query = query.OrderByDescending(x => x.CreatedAt);

            var totalItems = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items = logs,
                totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                currentPage = page,
                pageSize
            });
        }
    }
}