using CampusRooms.Api.Data;
using CampusRooms.Api.DTOs.Auth;
using CampusRooms.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Services;

public class AuthService(AppDbContext db) : IAuthService
{
    public async Task<ServiceResult<UserResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<UserResponseDto>.Fail("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult<UserResponseDto>.Fail("Password is required.");
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return ServiceResult<UserResponseDto>.Fail("Invalid email or password.");
        }

        return ServiceResult<UserResponseDto>.Ok(new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Nrp = user.Nrp,
            Email = user.Email,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc
        });
    }

    public async Task<ServiceResult<UserResponseDto>> RegisterAsync(RegisterUserRequestDto request)
    {
        var name = request.Name.Trim();
        var nrp = request.Nrp.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<UserResponseDto>.Fail("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(nrp))
        {
            return ServiceResult<UserResponseDto>.Fail("NRP is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<UserResponseDto>.Fail("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return ServiceResult<UserResponseDto>.Fail("Password must be at least 8 characters.");
        }

        var nrpExists = await db.Users.AnyAsync(x => x.Nrp == nrp);
        if (nrpExists)
        {
            return ServiceResult<UserResponseDto>.Fail("NRP is already registered.");
        }

        var emailExists = await db.Users.AnyAsync(x => x.Email == email);
        if (emailExists)
        {
            return ServiceResult<UserResponseDto>.Fail("Email is already registered.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            Nrp = nrp,
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = UserRole.User,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(user);
        db.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            EntityType = "User",
            EntityId = user.Id,
            EventType = "Registered",
            Actor = user.Email,
            Details = $"User {user.Email} registered with role {user.Role}",
            CreatedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return ServiceResult<UserResponseDto>.Ok(new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Nrp = user.Nrp,
            Email = user.Email,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc
        });
    }
}
