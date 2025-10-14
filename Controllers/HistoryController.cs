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
        private readonly TimeZoneInfo _manilaTimeZone;
        private readonly AppDbContext _context;

        public HistoryController(AppDbContext context)
        {
            _context = context;
            _manilaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        }

        // Helper method to convert UTC to Manila time
        private DateTime ConvertToManilaTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _manilaTimeZone);
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
                    // Basic field searches
                    EF.Functions.Like(x.EntityType.ToLower(), "%" + s + "%") ||
                    EF.Functions.Like(x.Action.ToLower(), "%" + s + "%") ||
                    EF.Functions.Like(x.ActorType.ToLower(), "%" + s + "%") ||
                    (x.EntityId.HasValue && EF.Functions.Like(x.EntityId.ToString(), "%" + s + "%")) ||
                    (x.ActorId.HasValue && EF.Functions.Like(x.ActorId.ToString(), "%" + s + "%")) ||

                    // Enhanced date/time search
                    EF.Functions.Like(x.CreatedAt.ToString("MM/dd/yyyy, h:mm:ss tt"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MM/dd/yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("M/d/yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MM/dd/yyyy HH:mm:ss"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MMMM dd, yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("MMM dd, yyyy"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("yyyy-MM-dd"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("HH:mm"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("h:mm tt"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("h:mm"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("h tt"), "%" + s + "%") ||
                    EF.Functions.Like(x.CreatedAt.ToString("HH:mm:ss"), "%" + s + "%") ||

                    // Enhanced DetailsJson search - parse JSON for specific fields
                    (x.DetailsJson != null && (
                        // Raw JSON search (fallback)
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%" + s + "%") ||

                        // Search for student names in JSON
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"studentName\":\"" + s + "%") ||
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"studentName\": \"" + s + "%") ||

                        // Search for titles in JSON
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"title\":\"" + s + "%") ||
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"title\": \"" + s + "%") ||

                        // Search for descriptions in JSON
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"description\":\"" + s + "%") ||
                        EF.Functions.Like(x.DetailsJson.ToLower(), "%\"description\": \"" + s + "%")
                    ))
                );
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            var totalItems = await query.CountAsync();

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Convert timestamps to Manila time before returning
            var logsWithManilaTime = logs.Select(log => new
            {
                log.ActivityId,
                log.EntityType,
                log.EntityId,
                log.Action,
                log.ActorType,
                log.ActorId,
                log.DetailsJson,
                CreatedAt = ConvertToManilaTime(log.CreatedAt) // Convert to Manila time
            }).ToList();

            return Ok(new
            {
                items = logsWithManilaTime,
                totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                currentPage = page,
                pageSize
            });
        }
    }
}