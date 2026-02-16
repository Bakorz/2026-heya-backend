using CampusRooms.Api.Data;
using CampusRooms.Api.Models;
using CampusRooms.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=campusrooms.db"));
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                return uri.Host == "localhost";
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("frontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var adminSeedTemplate = builder.Configuration.GetSection("AdminSeedTemplate").Get<AdminSeedTemplateOptions>();
    var hasAdmin = db.Users.Any(x => x.Role == UserRole.Admin);
    if (!hasAdmin && adminSeedTemplate?.Enabled == true)
    {
        var seedName = string.IsNullOrWhiteSpace(adminSeedTemplate.Name) ? "System Admin" : adminSeedTemplate.Name.Trim();
        var seedNrp = string.IsNullOrWhiteSpace(adminSeedTemplate.Nrp) ? "ADMIN001" : adminSeedTemplate.Nrp.Trim();
        var seedEmail = string.IsNullOrWhiteSpace(adminSeedTemplate.Email) ? "admin@campus.local" : adminSeedTemplate.Email.Trim().ToLowerInvariant();
        var seedPassword = string.IsNullOrWhiteSpace(adminSeedTemplate.Password) ? "Admin123!" : adminSeedTemplate.Password;

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = seedName,
            Nrp = seedNrp,
            Email = seedEmail,
            PasswordHash = PasswordHasher.Hash(seedPassword),
            Role = UserRole.Admin,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(admin);
        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "User",
            EntityId = admin.Id,
            EventType = "SeededAdmin",
            Actor = "system",
            Details = $"Default admin account created: {seedEmail}",
            CreatedAtUtc = DateTime.UtcNow
        });

        db.SaveChanges();
    }

    var userSeedTemplate = builder.Configuration.GetSection("UserSeedTemplate").Get<UserSeedTemplateOptions>();
    if (userSeedTemplate?.Enabled == true && userSeedTemplate.Users.Count > 0)
    {
        var now = DateTime.UtcNow;
        var existingNrps = db.Users
            .AsNoTracking()
            .Select(x => x.Nrp)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingEmails = db.Users
            .AsNoTracking()
            .Select(x => x.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var usersToSeed = userSeedTemplate.Users
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x.Nrp))
            .Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .Where(x => !string.IsNullOrWhiteSpace(x.Password))
            .Where(x => !existingNrps.Contains(x.Nrp.Trim()))
            .Where(x => !existingEmails.Contains(x.Email.Trim().ToLowerInvariant()))
            .Select(x => new AppUser
            {
                Id = Guid.NewGuid(),
                Name = x.Name.Trim(),
                Nrp = x.Nrp.Trim(),
                Email = x.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(x.Password),
                Role = UserRole.User,
                CreatedAtUtc = now,
                ModifiedAtUtc = now
            })
            .ToList();

        if (usersToSeed.Count > 0)
        {
            db.Users.AddRange(usersToSeed);
            db.AuditEvents.Add(new AuditEvent
            {
                Id = Guid.NewGuid(),
                EntityType = "User",
                EntityId = Guid.Empty,
                EventType = "SeededUsers",
                Actor = "system",
                Details = $"Seeded {usersToSeed.Count} user(s) from UserSeedTemplate",
                CreatedAtUtc = now
            });
            db.SaveChanges();
        }
    }

    var roomSeedTemplate = builder.Configuration.GetSection("RoomSeedTemplate").Get<RoomSeedTemplateOptions>();
    if (roomSeedTemplate?.Enabled == true && roomSeedTemplate.Rooms.Count > 0)
    {
        var now = DateTime.UtcNow;
        var existingCodes = db.Rooms
            .AsNoTracking()
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roomsToSeed = roomSeedTemplate.Rooms
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Where(x => !existingCodes.Contains(x.Code.Trim()))
            .Select(x => new Room
            {
                Id = Guid.NewGuid(),
                Code = x.Code.Trim(),
                Name = string.IsNullOrWhiteSpace(x.Name) ? x.Code.Trim() : x.Name.Trim(),
                Building = string.IsNullOrWhiteSpace(x.Building) ? "Unknown" : x.Building.Trim(),
                Capacity = x.Capacity <= 0 ? 1 : x.Capacity,
                IsActive = x.IsActive,
                CreatedAtUtc = now,
                ModifiedAtUtc = now
            })
            .ToList();

        if (roomsToSeed.Count > 0)
        {
            db.Rooms.AddRange(roomsToSeed);
            db.AuditEvents.Add(new AuditEvent
            {
                Id = Guid.NewGuid(),
                EntityType = "Room",
                EntityId = Guid.Empty,
                EventType = "SeededRooms",
                Actor = "system",
                Details = $"Seeded {roomsToSeed.Count} room(s) from RoomSeedTemplate",
                CreatedAtUtc = now
            });
            db.SaveChanges();
        }
    }
}

app.MapControllers();

app.Run();

public sealed class AdminSeedTemplateOptions
{
    public bool Enabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nrp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RoomSeedTemplateOptions
{
    public bool Enabled { get; set; }
    public List<RoomSeedTemplateItem> Rooms { get; set; } = [];
}

public sealed class UserSeedTemplateOptions
{
    public bool Enabled { get; set; }
    public List<UserSeedTemplateItem> Users { get; set; } = [];
}

public sealed class UserSeedTemplateItem
{
    public string Name { get; set; } = string.Empty;
    public string Nrp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RoomSeedTemplateItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}
