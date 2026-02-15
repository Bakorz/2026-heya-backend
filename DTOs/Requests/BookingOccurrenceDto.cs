using CampusRooms.Api.Models;

namespace CampusRooms.Api.DTOs.Requests;

public sealed record BookingOccurrenceDto(
    Guid Id,
    Guid BookingRequestId,
    Guid RoomId,
    DateTime StartUtc,
    DateTime EndUtc,
    RequestStatus Status,
    DateTime CreatedAtUtc);
