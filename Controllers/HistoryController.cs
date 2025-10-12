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
            [FromQuery] string? search,            // NEW
            [FromQuery] long? entityId = null,     // optional
            [FromQuery] long? actorId = null,      // optional
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

            if (entityId.HasValue)
                query = query.Where(x => x.EntityId == entityId.Value);

            if (actorId.HasValue)
                query = query.Where(x => x.ActorId == actorId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    // Search in EntityType (Record Type column)
                    EF.Functions.Like(x.EntityType.ToLower(), "%" + s + "%") ||

                    // Search in Action (Action column)
                    EF.Functions.Like(x.Action.ToLower(), "%" + s + "%") ||

                    // Search in ActorType (Performed By column)
                    EF.Functions.Like(x.ActorType.ToLower(), "%" + s + "%") ||

                    // Search in EntityId (Record Type column - ID numbers)
                    (x.EntityId.HasValue && EF.Functions.Like(x.EntityId.ToString(), "%" + s + "%")) ||

                    // Search in ActorId (Performed By column - ID numbers)
                    (x.ActorId.HasValue && EF.Functions.Like(x.ActorId.ToString(), "%" + s + "%")) ||

                    // Search in DetailsJson (Student names, titles, descriptions)
                    (x.DetailsJson != null && EF.Functions.Like(x.DetailsJson.ToLower(), "%" + s + "%")) ||

                    // Search in formatted date/time (Date/Time column)
                    EF.Functions.Like(x.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MMMM dd, yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MMM dd, yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("yyyy-MM-dd"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("HH:mm"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("h:mm tt"), "%" + s + "%")
                );
            }

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