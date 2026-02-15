using CampusRooms.Api.Data;
using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Services;

public class RequestService(AppDbContext db) : IRequestService
{
    private static readonly TimeOnly WorkingDayStart = new(8, 0);
    private static readonly TimeOnly WorkingDayEnd = new(20, 0);

    public async Task<IReadOnlyList<BookingRequestResponseDto>> GetRequestsAsync(
        string? requestedBy,
        RequestStatus? status,
        string? search,
        string? sortBy,
        bool desc)
    {
        var query = db.BookingRequests
            .AsNoTracking()
            .Include(x => x.Room)
            .Include(x => x.Occurrences)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(requestedBy))
        {
            query = query.Where(x => x.RequestedBy == requestedBy);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Purpose.Contains(search) ||
                x.RequestedBy.Contains(search) ||
                (x.Room != null && x.Room.Name.Contains(search)));
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            "start" => desc ? query.OrderByDescending(x => x.StartUtc) : query.OrderBy(x => x.StartUtc),
            _ => desc ? query.OrderByDescending(x => x.ModifiedAtUtc) : query.OrderBy(x => x.ModifiedAtUtc)
        };

        var rows = await query.Take(300).ToListAsync();
        return rows.Select(x => x.ToDto()).ToList();
    }

    public async Task<ServiceResult<BookingRequestResponseDto>> CreateRequestAsync(CreateBookingRequestDto request)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId && x.IsActive);
        if (room is null)
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("Room not found or inactive.");
        }

        if (request.AttendeeCount > room.Capacity)
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("Attendee count exceeds room capacity.");
        }

        if (request.EndUtc <= request.StartUtc)
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("End time must be after start time.");
        }

        var occurrences = BuildOccurrences(request.StartUtc, request.EndUtc, request.RecurrencePattern, request.RecurrenceUntilUtc);
        if (occurrences.Count == 0)
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("Unable to generate request occurrences.");
        }

        if (occurrences.Any(x => !IsWithinWorkingHours(x.StartUtc, x.EndUtc)))
        {
            return ServiceResult<BookingRequestResponseDto>.Fail("All occurrences must be within working hours (08:00-20:00 UTC). ");
        }

        foreach (var occurrence in occurrences)
        {
            var overlap = await db.BookingOccurrences.AnyAsync(x =>
                x.RoomId == request.RoomId &&
                x.Status == RequestStatus.Approved &&
                occurrence.StartUtc < x.EndUtc &&
                occurrence.EndUtc > x.StartUtc);

            if (overlap)
            {
                return ServiceResult<BookingRequestResponseDto>.Fail($"Time conflict found at {occurrence.StartUtc:u} - {occurrence.EndUtc:u}.");
            }
        }

        var bookingRequest = new BookingRequest
        {
            Id = Guid.NewGuid(),
            RoomId = request.RoomId,
            RequestedBy = request.RequestedBy.Trim(),
            Purpose = request.Purpose.Trim(),
            AttendeeCount = request.AttendeeCount,
            StartUtc = request.StartUtc,
            EndUtc = request.EndUtc,
            RecurrencePattern = request.RecurrencePattern,
            RecurrenceUntilUtc = request.RecurrenceUntilUtc,
            Status = RequestStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedAtUtc = DateTime.UtcNow,
            Occurrences = occurrences.Select(x => new BookingOccurrence
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                StartUtc = x.StartUtc,
                EndUtc = x.EndUtc,
                Status = RequestStatus.Submitted,
                CreatedAtUtc = DateTime.UtcNow
            }).ToList()
        };

        db.BookingRequests.Add(bookingRequest);
        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "BookingRequest",
            EntityId = bookingRequest.Id,
            EventType = "Submitted",
            Actor = request.RequestedBy,
            Details = $"Request submitted with {bookingRequest.Occurrences.Count} occurrence(s)",
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        bookingRequest.Room = room;
        return ServiceResult<BookingRequestResponseDto>.Ok(bookingRequest.ToDto());
    }

    private static bool IsWithinWorkingHours(DateTime startUtc, DateTime endUtc)
    {
        var start = TimeOnly.FromDateTime(startUtc);
        var end = TimeOnly.FromDateTime(endUtc);
        return start >= WorkingDayStart && end <= WorkingDayEnd;
    }

    private static List<(DateTime StartUtc, DateTime EndUtc)> BuildOccurrences(
        DateTime startUtc,
        DateTime endUtc,
        RecurrencePattern recurrencePattern,
        DateTime? recurrenceUntilUtc)
    {
        var output = new List<(DateTime StartUtc, DateTime EndUtc)> { (startUtc, endUtc) };

        if (recurrencePattern == RecurrencePattern.None || recurrenceUntilUtc is null)
        {
            return output;
        }

        var cursorStart = startUtc;
        var cursorEnd = endUtc;

        while (true)
        {
            (cursorStart, cursorEnd) = recurrencePattern switch
            {
                RecurrencePattern.Daily => (cursorStart.AddDays(1), cursorEnd.AddDays(1)),
                RecurrencePattern.Weekly => (cursorStart.AddDays(7), cursorEnd.AddDays(7)),
                _ => (cursorStart, cursorEnd)
            };

            if (cursorStart > recurrenceUntilUtc.Value)
            {
                break;
            }

            output.Add((cursorStart, cursorEnd));

            if (output.Count > 366)
            {
                break;
            }
        }

        return output;
    }
}
