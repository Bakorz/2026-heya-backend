namespace CampusRooms.Api.DTOs.Analytics;

public sealed record StatusCountDto(string Status, int Count);

public sealed record AnalyticsSummaryDto(
    int TotalRooms,
    int ActiveRooms,
    IReadOnlyList<StatusCountDto> RequestCounts);
