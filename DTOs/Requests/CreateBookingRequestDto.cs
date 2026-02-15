using CampusRooms.Api.Models;

namespace CampusRooms.Api.DTOs.Requests;

public sealed record CreateBookingRequestDto(
    Guid RoomId,
    string RequestedBy,
    string Purpose,
    int AttendeeCount,
    DateTime StartUtc,
    DateTime EndUtc,
    RecurrencePattern RecurrencePattern,
    DateTime? RecurrenceUntilUtc);
