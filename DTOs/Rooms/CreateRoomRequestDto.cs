namespace CampusRooms.Api.DTOs.Rooms;

public sealed record CreateRoomRequestDto(
    string Code,
    string Name,
    string Building,
    int Capacity,
    string? Actor);
