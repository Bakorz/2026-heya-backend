using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.Models;

namespace CampusRooms.Api.Services;

public interface IRequestService
{
    Task<IReadOnlyList<BookingRequestResponseDto>> GetRequestsAsync(
        string? requestedBy,
        RequestStatus? status,
        string? search,
        string? sortBy,
        bool desc);

    Task<ServiceResult<BookingRequestResponseDto>> CreateRequestAsync(CreateBookingRequestDto request);
}
