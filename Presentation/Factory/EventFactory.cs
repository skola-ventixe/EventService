using Presentation.Models;

namespace Presentation.Factory;

public static class EventFactory
{
    public static Event CreateEvent()
    {
        return new Event();
    }
    
    public static Event CreateEvent(EventRegistrationDto regDto)
    {
        return new Event
        {
            EventName = regDto.EventName,
            EventDescription = regDto.EventDescription,
            Venue = regDto.Venue,
            StreetAddress = regDto.StreetAddress,
            City = regDto.City,
            ZipCode = regDto.ZipCode,
            State = regDto.State,
            Country = regDto.Country,
            StartDate = regDto.StartDate,
            EndDate = regDto.EndDate,
            TicketSalesStart = regDto.TicketSalesStart,
            EventImageUrl = regDto.EventImageUrl
        };
    }

    public static EventRegistrationDto CreateRegistrationForm()
    {
        return new EventRegistrationDto();
    }


}
