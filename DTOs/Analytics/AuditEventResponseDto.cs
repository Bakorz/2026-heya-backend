namespace CampusRooms.Api.DTOs.Analytics;

public sealed record AuditEventResponseDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string EventType,
    string Actor,
    string? Details,
    DateTime CreatedAtUtc);
