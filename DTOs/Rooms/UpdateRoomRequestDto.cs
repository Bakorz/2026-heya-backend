namespace CampusRooms.Api.DTOs.Rooms;

public sealed record UpdateRoomRequestDto(
    string Name,
    string Building,
    int Capacity,
    string? Actor);
