namespace CampusRooms.Api.DTOs.Rooms;

public sealed record RoomResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Building,
    int Capacity,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime ModifiedAtUtc);
