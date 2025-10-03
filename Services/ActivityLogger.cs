using System.Text.Json;
using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Models;

namespace GuidanceOfficeAPI.Services
{
    public interface IActivityLogger
    {
        Task LogAsync(string entityType, long? entityId, string action, string actorType, long? actorId, object? details = null);
    }

    public class ActivityLogger : IActivityLogger
    {
        private readonly AppDbContext _ctx;
        public ActivityLogger(AppDbContext ctx) { _ctx = ctx; }

        public async Task LogAsync(string entityType, long? entityId, string action, string actorType, long? actorId, object? details = null)
        {
            var log = new ActivityLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                ActorType = actorType,
                ActorId = actorId,
                DetailsJson = details == null ? null : JsonSerializer.Serialize(details),
                CreatedAt = DateTime.UtcNow
            };
            _ctx.ActivityLogs.Add(log);
            await _ctx.SaveChangesAsync();
        }
    }
}
