namespace CampusRooms.Api.DTOs.Auth;

public sealed record LoginRequestDto(
    string Email,
    string Password);
