using CampusRooms.Api.Data;
using CampusRooms.Api.DTOs.Analytics;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Services;

public class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public async Task<IReadOnlyList<AuditEventResponseDto>> GetEventsAsync(
        string? entityType,
        string? eventType,
        string? actor,
        string? search,
        string? sortBy,
        bool desc)
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
        return rows.Select(x => x.ToDto()).ToList();
    }

    public async Task<AnalyticsSummaryDto> GetSummaryAsync()
    {
        var requestCounts = await db.BookingRequests
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(x => new StatusCountDto(x.Key.ToString(), x.Count()))
            .ToListAsync();

        var totalRooms = await db.Rooms.CountAsync();
        var activeRooms = await db.Rooms.CountAsync(x => x.IsActive);

        return new AnalyticsSummaryDto(totalRooms, activeRooms, requestCounts);
    }
}
