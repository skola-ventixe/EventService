using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Presentation.Data;
using Presentation.Factory;
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

    public async Task<ServiceResponse<Event>> GetEventByIdAsync(string id)
    {
        try
        {
            var eventEntity = await _set.FindAsync(id);
            if (eventEntity == null)
            {
                return new ServiceResponse<Event>
                {
                    Success = false,
                    Message = $"Event with ID {id} not found."
                };
            }
            return new ServiceResponse<Event>
            {
                Success = true,
                Data = eventEntity
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponse<Event>
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<EventInfoDto?> GetEventInfoByIdAsync(string id)
    {
        EventInfoDto? returnedEvent;
        try
        {
            var eventEntity = await _set.FindAsync(id);
            if (eventEntity == null)
            {
                Console.WriteLine($"Event with ID {id} not found.");
                return null;
            }

            returnedEvent = EventFactory.CreateEvent(eventEntity);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            return null;
        }

        var serializerOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Fetch packages for the event
        try
        {
            var packageResponse = await _httpClient.GetAsync($"https://ventixepackageservice-bnbkbjajh9a4ehhh.swedencentral-01.azurewebsites.net/api/packages/event/{id}");
            if (packageResponse.IsSuccessStatusCode)
            {
                var content = await packageResponse.Content.ReadAsStringAsync();

                var packages = System.Text.Json.JsonSerializer.Deserialize<List<Package>>(content, serializerOptions);
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

        // Fetch attendees count for the event
        try
        {
            var response = await _httpClient.GetAsync($"https://ventixeticketservice-gzehf9d8cffzfwed.swedencentral-01.azurewebsites.net/api/tickets/sold/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var attendeesCount = System.Text.Json.JsonSerializer.Deserialize<int>(content, serializerOptions);
                    returnedEvent.AttendeesCount = attendeesCount;
            }
            else
            {
                returnedEvent.AttendeesCount = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching attendees for event {id}: {ex.Message}");
            returnedEvent.AttendeesCount = 0;
        }
        return returnedEvent;
    }


    public async Task<ServiceResponse<EventInfoDto?>> AddEventAsync(EventRegistrationDto newEvent)
    {
        if (newEvent == null)
        {
            return new ServiceResponse<EventInfoDto?>
            {
                Success = false,
                Error = "Event cannot be null"
            };
        }

        var entity = EventFactory.CreateEvent(newEvent);

        try
        {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            return new ServiceResponse<EventInfoDto?>
            {
                Success = false,
                Error = $"Failed to create event: {ex.Message}"
            };
        }

        List<Package>? packageResponse = null;
        try
        {
            if (newEvent.Packages != null && newEvent.Packages.Count > 0)
            {
                List<PackageRegistrationDto> packages = [.. newEvent.Packages.Select(p => new PackageRegistrationDto
                {
                    EventId = entity.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Benefits = p.Benefits?.Select(b => new Benefit
                    {
                        PackageId = entity.Id, // Set the PackageId to the new package's id
                        Description = b.Description,
                    }).ToList(),
                    Placement = p.Placement,
                    Seated = p.Seated,
                })];

                var jsonData = await _httpClient.PostAsJsonAsync("https://ventixepackageservice-bnbkbjajh9a4ehhh.swedencentral-01.azurewebsites.net/api/packages", packages);
                if (!jsonData.IsSuccessStatusCode)
                {
                    _set.Remove(entity);
                    await _context.SaveChangesAsync();
                    return new ServiceResponse<EventInfoDto?>
                    {
                        Success = false,
                        Error = "Failed to create packages for the event."
                    };
                }
                packageResponse = await jsonData.Content.ReadFromJsonAsync<List<Package>>();
            }
        }
        catch(Exception ex) {
            return new ServiceResponse<EventInfoDto?>
            {
                Success = false,
                Error = $"Failed to create packages: {ex.Message}"
            };
        }

        var eventInfoDto = EventFactory.CreateEvent(entity);
        if (packageResponse != null && packageResponse.Count > 0)
        {
            eventInfoDto.Packages = packageResponse;
        }
        return new ServiceResponse<EventInfoDto?>
        {
            Success = true,
            Message = "Event created successfully",
            Data = eventInfoDto
        };
    }




    public async Task UpdateEventAsync(Event updatedEvent)
    {
        _set.Update(updatedEvent);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteEventAsync(string id)
    {
        var eventToDelete = (await GetEventByIdAsync(id)).Data;
        if (eventToDelete != null)
        {
            _set.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
