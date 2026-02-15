using CampusRooms.Api.DTOs.Analytics;

namespace CampusRooms.Api.Services;

public interface IAnalyticsService
{
    Task<IReadOnlyList<AuditEventResponseDto>> GetEventsAsync(
        string? entityType,
        string? eventType,
        string? actor,
        string? search,
        string? sortBy,
        bool desc);

    Task<AnalyticsSummaryDto> GetSummaryAsync();
}
