using CampusRooms.Api.Data;
using CampusRooms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Features;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/analytics");

        group.MapGet("/events", async (
            AppDbContext db,
            string? entityType,
            string? eventType,
            string? actor,
            string? search,
            string? sortBy,
            bool desc = true) =>
        {
            var query = db.AuditEvents.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(x => x.EntityType == entityType);
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(x => x.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(actor))
            {
                query = query.Where(x => x.Actor == actor);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.EventType.Contains(search) ||
                    x.EntityType.Contains(search) ||
                    x.Actor.Contains(search) ||
                    (x.Details != null && x.Details.Contains(search)));
            }

            query = (sortBy?.ToLowerInvariant()) switch
            {
                "eventtype" => desc ? query.OrderByDescending(x => x.EventType) : query.OrderBy(x => x.EventType),
                "actor" => desc ? query.OrderByDescending(x => x.Actor) : query.OrderBy(x => x.Actor),
                _ => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc)
            };

            var rows = await query.Take(500).ToListAsync();
            return Results.Ok(rows);
        });

        group.MapGet("/summary", async (AppDbContext db) =>
        {
            var requestCounts = await db.BookingRequests
                .AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key.ToString(), Count = x.Count() })
                .ToListAsync();

            var totalRooms = await db.Rooms.CountAsync();
            var activeRooms = await db.Rooms.CountAsync(x => x.IsActive);

            return Results.Ok(new
            {
                TotalRooms = totalRooms,
                ActiveRooms = activeRooms,
                RequestCounts = requestCounts
            });
        });

        return group;
    }
}
