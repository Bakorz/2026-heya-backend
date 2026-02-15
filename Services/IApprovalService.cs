using CampusRooms.Api.DTOs.Approvals;
using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.Models;

namespace CampusRooms.Api.Services;

public interface IApprovalService
{
    Task<IReadOnlyList<BookingRequestResponseDto>> GetQueueAsync(RequestStatus status);
    Task<ServiceResult<BookingRequestResponseDto>> DecideAsync(Guid requestId, DecideRequestDto request);
}
