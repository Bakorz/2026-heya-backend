using CampusRooms.Api.Data;
using CampusRooms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Features;

public static class RoomEndpoints
{
    public static RouteGroupBuilder MapRoomEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/rooms");

        group.MapGet("", async (AppDbContext db, string? search, bool? isActive) =>
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

            return Results.Ok(rooms);
        });

        group.MapPost("", async (AppDbContext db, CreateRoomRequest request) =>
        {
            if (request.Capacity <= 0)
            {
                return Results.BadRequest("Capacity must be greater than zero.");
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
            return Results.Created($"/api/rooms/{room.Id}", room);
        });

        group.MapPut("/{id:guid}", async (AppDbContext db, Guid id, UpdateRoomRequest request) =>
        {
            var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (room is null)
            {
                return Results.NotFound();
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
            return Results.Ok(room);
        });

        group.MapPatch("/{id:guid}/active", async (AppDbContext db, Guid id, SetRoomActiveRequest request) =>
        {
            var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (room is null)
            {
                return Results.NotFound();
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
            return Results.Ok(room);
        });

        return group;
    }

    public sealed record CreateRoomRequest(string Code, string Name, string Building, int Capacity, string? Actor);
    public sealed record UpdateRoomRequest(string Name, string Building, int Capacity, string? Actor);
    public sealed record SetRoomActiveRequest(bool IsActive, string? Actor);
}
