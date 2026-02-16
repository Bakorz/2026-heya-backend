using CampusRooms.Api.DTOs.Auth;

namespace CampusRooms.Api.Services;

public interface IAuthService
{
    Task<ServiceResult<UserResponseDto>> RegisterAsync(RegisterUserRequestDto request);
    Task<ServiceResult<UserResponseDto>> LoginAsync(LoginRequestDto request);
}
