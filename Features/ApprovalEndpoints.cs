using CampusRooms.Api.Data;
using CampusRooms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Features;

public static class ApprovalEndpoints
{
    public static RouteGroupBuilder MapApprovalEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/approvals");

        group.MapGet("/queue", async (AppDbContext db, RequestStatus status = RequestStatus.Submitted) =>
        {
            var queue = await db.BookingRequests
                .AsNoTracking()
                .Include(x => x.Room)
                .Include(x => x.Occurrences)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.ModifiedAtUtc)
                .Take(200)
                .ToListAsync();

            return Results.Ok(queue);
        });

        group.MapPost("/{requestId:guid}/decide", async (AppDbContext db, Guid requestId, DecideRequest request) =>
        {
            var bookingRequest = await db.BookingRequests
                .Include(x => x.Occurrences)
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (bookingRequest is null)
            {
                return Results.NotFound();
            }

            if (bookingRequest.Status is RequestStatus.Approved or RequestStatus.Rejected)
            {
                return Results.BadRequest("Request already finalized.");
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
                        return Results.BadRequest($"Cannot approve due to overlap at {occurrence.StartUtc:u}.");
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
            return Results.Ok(bookingRequest);
        });

        return group;
    }

    public sealed record DecideRequest(string AdminName, bool IsApproved, string? Comment);
}
