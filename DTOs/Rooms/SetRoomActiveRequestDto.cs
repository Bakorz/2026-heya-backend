namespace CampusRooms.Api.DTOs.Rooms;

public sealed record SetRoomActiveRequestDto(bool IsActive, string? Actor);
