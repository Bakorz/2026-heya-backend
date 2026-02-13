namespace CampusRooms.Api.Domain;

public class BookingRequest
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public int AttendeeCount { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
    public DateTime? RecurrenceUntilUtc { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public string? AdminComment { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<BookingOccurrence> Occurrences { get; set; } = new List<BookingOccurrence>();
    public ICollection<ApprovalDecision> Decisions { get; set; } = new List<ApprovalDecision>();
}
