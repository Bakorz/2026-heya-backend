using CampusRooms.Api.Data;
using CampusRooms.Api.DTOs.Approvals;
using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Services;

public class ApprovalService(AppDbContext db) : IApprovalService
{
    public async Task<IReadOnlyList<BookingRequestResponseDto>> GetQueueAsync(RequestStatus status)
    {
        var queue = await db.BookingRequests
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.Occurrences)
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.ModifiedAtUtc)
            .Take(200)
            .ToListAsync();

        return queue.Select(x => x.ToDto()).ToList();
    }

    public async Task<ServiceResult<BookingRequestResponseDto>> DecideAsync(Guid requestId, DecideRequestDto request)
    {
        var bookingRequest = await db.BookingRequests
            .Include(x => x.Room)
            .Include(x => x.Occurrences)
            .FirstOrDefaultAsync(x => x.Id == requestId);

        if (bookingRequest is null)
        {
            return ServiceResult<BookingRequestResponseDto>.NotFoundResult();
        }

        if (bookingRequest.Status is RequestStatus.Approved or RequestStatus.Rejected)
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("Request already finalized.");
        }

        if (request.IsApproved)
        {
            foreach (var occurrence in bookingRequest.Occurrences)
            {
                var hasOverlap = await db.BookingOccurrences.AnyAsync(x =>
                    x.Id != occurrence.Id &&
                    x.RoomId == occurrence.RoomId &&
                    x.Status == RequestStatus.Approved &&
                    occurrence.StartUtc < x.EndUtc &&
                    occurrence.EndUtc > x.StartUtc);

                if (hasOverlap)
                {
                    return ServiceResult<BookingRequestResponseDto>.Fail($"Cannot approve due to overlap at {occurrence.StartUtc:u}.");
                }
            }
        }

        var newStatus = request.IsApproved ? RequestStatus.Approved : RequestStatus.Rejected;
        bookingRequest.Status = newStatus;
        bookingRequest.AdminComment = request.Comment;
        bookingRequest.ModifiedAtUtc = DateTime.UtcNow;

        foreach (var occurrence in bookingRequest.Occurrences)
        {
            occurrence.Status = newStatus;
        }

        db.ApprovalDecisions.Add(new ApprovalDecision
        {
            Id = Guid.NewGuid(),
            BookingRequestId = bookingRequest.Id,
            AdminName = request.AdminName,
            IsApproved = request.IsApproved,
            Comment = request.Comment,
            DecidedAtUtc = DateTime.UtcNow
        });

        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "BookingRequest",
            EntityId = bookingRequest.Id,
            EventType = request.IsApproved ? "Approved" : "Rejected",
            Actor = request.AdminName,
            Details = request.Comment,
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return ServiceResult<BookingRequestResponseDto>.Ok(bookingRequest.ToDto());
    }
}
