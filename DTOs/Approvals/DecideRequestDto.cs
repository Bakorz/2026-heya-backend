namespace CampusRooms.Api.DTOs.Approvals;

public sealed record DecideRequestDto(string AdminName, bool IsApproved, string? Comment);
