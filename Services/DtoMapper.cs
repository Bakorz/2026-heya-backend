using CampusRooms.Api.DTOs.Analytics;
using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.DTOs.Rooms;
using CampusRooms.Api.Models;

namespace CampusRooms.Api.Services;

internal static class DtoMapper
{
    public static RoomResponseDto ToDto(this Room room) => new(
        room.Id,
        room.Code,
        room.Name,
        room.Building,
        room.Capacity,
        room.IsActive,
        room.CreatedAtUtc,
        room.ModifiedAtUtc);

    public static BookingOccurrenceDto ToDto(this BookingOccurrence occurrence) => new(
        occurrence.Id,
        occurrence.BookingRequestId,
        occurrence.RoomId,
        occurrence.StartUtc,
        occurrence.EndUtc,
        occurrence.Status,
        occurrence.CreatedAtUtc);

    public static BookingRequestResponseDto ToDto(this BookingRequest request) => new(
        request.Id,
        request.RoomId,
        request.Room is null ? null : request.Room.ToDto(),
        request.RequestedBy,
        request.Purpose,
        request.AttendeeCount,
        request.StartUtc,
        request.EndUtc,
        request.RecurrencePattern,
        request.RecurrenceUntilUtc,
        request.Status,
        request.AdminComment,
        request.CreatedAtUtc,
        request.ModifiedAtUtc,
        request.Occurrences
            .OrderBy(x => x.StartUtc)
            .Select(x => x.ToDto())
            .ToList());

    public static AuditEventResponseDto ToDto(this AuditEvent auditEvent) => new(
        auditEvent.Id,
        auditEvent.EntityType,
        auditEvent.EntityId,
        auditEvent.EventType,
        auditEvent.Actor,
        auditEvent.Details,
        auditEvent.CreatedAtUtc);
}
