using CampusRooms.Api.DTOs.Rooms;

namespace CampusRooms.Api.Services;

public interface IRoomService
{
    Task<IReadOnlyList<RoomResponseDto>> GetRoomsAsync(string? search, bool? isActive);
    Task<ServiceResult<RoomResponseDto>> CreateRoomAsync(CreateRoomRequestDto request);
    Task<ServiceResult<RoomResponseDto>> UpdateRoomAsync(Guid id, UpdateRoomRequestDto request);
    Task<ServiceResult<RoomResponseDto>> SetActiveAsync(Guid id, SetRoomActiveRequestDto request);
}
