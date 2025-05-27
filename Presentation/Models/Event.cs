namespace Presentation.Models;

public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
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
    public List<Package>? Packages { get; set; } = null!;



}
