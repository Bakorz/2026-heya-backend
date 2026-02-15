using CampusRooms.Api.DTOs.Rooms;
using CampusRooms.Api.Models;

namespace CampusRooms.Api.DTOs.Requests;

public sealed record BookingRequestResponseDto(
    Guid Id,
    Guid RoomId,
    RoomResponseDto? Room,
    string RequestedBy,
    string Purpose,
    int AttendeeCount,
    DateTime StartUtc,
    DateTime EndUtc,
    RecurrencePattern RecurrencePattern,
    DateTime? RecurrenceUntilUtc,
    RequestStatus Status,
    string? AdminComment,
    DateTime CreatedAtUtc,
    DateTime ModifiedAtUtc,
    IReadOnlyList<BookingOccurrenceDto> Occurrences);
