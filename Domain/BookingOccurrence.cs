namespace CampusRooms.Api.Domain;

public class BookingOccurrence
{
    public Guid Id { get; set; }
    public Guid BookingRequestId { get; set; }
    public BookingRequest? BookingRequest { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Submitted;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
