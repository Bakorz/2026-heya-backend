namespace CampusRooms.Api.Domain;

public class AuditEvent
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
