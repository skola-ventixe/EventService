namespace Presentation.Models;

public class EventInfoDto
{
    public string Id { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public string? EventDescription { get; set; }
    public string Venue { get; set; } = null!;
    public string? StreetAddress { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? ZipCode { get; set; } = null!;
    public string? State { get; set; } = null!;
    public string? Country { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime TicketSalesStart { get; set; }
    public string? EventImageUrl { get; set; }
    public int MaxAttendees { get; set; }
    public int AttendeesCount { get; set; }
    public List<Package>? Packages { get; set; } = null!;
}
