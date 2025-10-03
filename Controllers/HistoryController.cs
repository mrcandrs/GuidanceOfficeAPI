using GuidanceOfficeAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/history")]
    public class HistoryController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public HistoryController(AppDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? entityType,
            [FromQuery] long? entityId,
            [FromQuery] string? action,
            [FromQuery] string? actorType,
            [FromQuery] long? actorId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var q = _ctx.ActivityLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(x => x.EntityType == entityType);
            if (entityId.HasValue) q = q.Where(x => x.EntityId == entityId);
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(x => x.Action == action);
            if (!string.IsNullOrWhiteSpace(actorType)) q = q.Where(x => x.ActorType == actorType);
            if (actorId.HasValue) q = q.Where(x => x.ActorId == actorId);
            if (from.HasValue) q = q.Where(x => x.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(x => x.CreatedAt <= to.Value);

            q = q.OrderByDescending(x => x.CreatedAt);
            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(new { total, page, pageSize, items });
        }
    }
}