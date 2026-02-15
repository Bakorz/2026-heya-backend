using CampusRooms.Api.Data;
using CampusRooms.Api.DTOs.Rooms;
using CampusRooms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Services;

public class RoomService(AppDbContext db) : IRoomService
{
    public async Task<IReadOnlyList<RoomResponseDto>> GetRoomsAsync(string? search, bool? isActive)
    {
        var query = db.Rooms.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search) ||
                x.Building.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var rooms = await query
            .OrderBy(x => x.Building)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return rooms.Select(x => x.ToDto()).ToList();
    }

    public async Task<ServiceResult<RoomResponseDto>> CreateRoomAsync(CreateRoomRequestDto request)
    {
        if (request.Capacity <= 0)
        {
            return ServiceResult<RoomResponseDto>.Fail("Capacity must be greater than zero.");
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Building = request.Building.Trim(),
            Capacity = request.Capacity,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedAtUtc = DateTime.UtcNow
        };

        db.Rooms.Add(room);
        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "Room",
            EntityId = room.Id,
            EventType = "Created",
            Actor = request.Actor ?? "system",
            Details = $"Room {room.Code} created",
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return ServiceResult<RoomResponseDto>.Ok(room.ToDto());
    }

    public async Task<ServiceResult<RoomResponseDto>> UpdateRoomAsync(Guid id, UpdateRoomRequestDto request)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
        if (room is null)
        {
            return ServiceResult<RoomResponseDto>.NotFoundResult();
        }

        room.Name = request.Name.Trim();
        room.Building = request.Building.Trim();
        room.Capacity = request.Capacity;
        room.ModifiedAtUtc = DateTime.UtcNow;

        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "Room",
            EntityId = room.Id,
            EventType = "Updated",
            Actor = request.Actor ?? "system",
            Details = $"Room {room.Code} updated",
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return ServiceResult<RoomResponseDto>.Ok(room.ToDto());
    }

    public async Task<ServiceResult<RoomResponseDto>> SetActiveAsync(Guid id, SetRoomActiveRequestDto request)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
        if (room is null)
        {
            return ServiceResult<RoomResponseDto>.NotFoundResult();
        }

        room.IsActive = request.IsActive;
        room.ModifiedAtUtc = DateTime.UtcNow;

        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "Room",
            EntityId = room.Id,
            EventType = request.IsActive ? "Enabled" : "Disabled",
            Actor = request.Actor ?? "system",
            Details = $"Room {room.Code} {(request.IsActive ? "enabled" : "disabled")}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return ServiceResult<RoomResponseDto>.Ok(room.ToDto());
    }
}
