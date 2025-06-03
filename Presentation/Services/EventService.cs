using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Presentation.Data;
using Presentation.Models;

namespace Presentation.Services;

public class EventService
{
    private readonly EventDataContext _context;
    private readonly DbSet<Event> _set;
    private readonly HttpClient _httpClient;

    public EventService(EventDataContext context, HttpClient httpClient)
    {
        _context = context;
        _set = _context.Set<Event>();
        _httpClient = httpClient;
    }
    public async Task<ServiceResponse<List<Event>>> GetAllEventsAsync()
    {
        try
        {
            var events = await _set.ToListAsync();
            return new ServiceResponse<List<Event>>
            {
                Success = true,
                Message = "All events retrieved successfully",
                Data = await _set.ToListAsync()
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<List<Event>>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
    public async Task<Event?> GetEventByIdAsync(string id)
    {
        Event? returnedEvent;
        try
        {
            returnedEvent = await _set.FindAsync(id);
            if (returnedEvent == null)
                return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"https://ventixepackageservice-bnbkbjajh9a4ehhh.swedencentral-01.azurewebsites.net/api/packages/event/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var packages = System.Text.Json.JsonSerializer.Deserialize<List<Package>>(content);
                if (packages != null)
                    returnedEvent.Packages = packages;
            }
            else
            {
                returnedEvent.Packages = [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching packages for event {id}: {ex.Message}");
            returnedEvent.Packages = [];
        }
        return returnedEvent;
    }


    public async Task AddEventAsync(Event newEvent)
    {

        await _set.AddAsync(newEvent);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateEventAsync(Event updatedEvent)
    {
        _set.Update(updatedEvent);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteEventAsync(string id)
    {
        var eventToDelete = await GetEventByIdAsync(id);
        if (eventToDelete != null)
        {
            _set.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
