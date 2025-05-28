using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class EventRegistrationDto
{
    [Required]
    [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters.")]
    [MinLength(3, ErrorMessage = "Event name must be at least 3 characters long.")]
    public string EventName { get; set; } = null!;
    public string? EventDescription { get; set; }
    [Required]
    [StringLength(100, ErrorMessage = "Event name cannot exceed 100 characters.")]
    [MinLength(3, ErrorMessage = "Event name must be at least 3 characters long.")]
    public string Venue { get; set; } = null!;
    public string? StreetAddress { get; set; } = null!;
    [Required]
    [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
    [MinLength(3, ErrorMessage = "City name must be at least 3 characters long.")]
    public string City { get; set; } = null!;
    public string? ZipCode { get; set; } = null!;
    public string? State { get; set; } = null!;
    public string? Country { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime TicketSalesStart { get; set; }
    public string? EventImageUrl { get; set; }
    public int MaxAttendees { get; set; }
}
