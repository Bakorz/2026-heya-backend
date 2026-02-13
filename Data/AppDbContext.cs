using CampusRooms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();
    public DbSet<BookingOccurrence> BookingOccurrences => Set<BookingOccurrence>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Building).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.Status, x.ModifiedAtUtc });
            entity.Property(x => x.RequestedBy).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(500).IsRequired();

            entity
                .HasOne(x => x.Room)
                .WithMany(x => x.BookingRequests)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingOccurrence>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.RoomId, x.StartUtc, x.EndUtc, x.Status });

            entity
                .HasOne(x => x.BookingRequest)
                .WithMany(x => x.Occurrences)
                .HasForeignKey(x => x.BookingRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApprovalDecision>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.BookingRequestId, x.DecidedAtUtc });

            entity
                .HasOne(x => x.BookingRequest)
                .WithMany(x => x.Decisions)
                .HasForeignKey(x => x.BookingRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EntityType, x.EventType, x.CreatedAtUtc });
            entity.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Actor).HasMaxLength(128).IsRequired();
        });
    }
}
