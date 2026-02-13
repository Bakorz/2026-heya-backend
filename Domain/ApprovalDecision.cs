namespace CampusRooms.Api.Domain;

public class ApprovalDecision
{
    public Guid Id { get; set; }
    public Guid BookingRequestId { get; set; }
    public BookingRequest? BookingRequest { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? Comment { get; set; }
    public DateTime DecidedAtUtc { get; set; } = DateTime.UtcNow;
}
