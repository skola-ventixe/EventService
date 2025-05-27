using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Presentation.Data;
using Presentation.Models;

namespace Presentation.Services;

public class EventService
{
    private readonly DataContext _context;
    private readonly DbSet<Event> _set;
    private readonly EventBusListener _eventBusListener;

    public EventService(DataContext context, EventBusListener eventBusListener)
    {
        _context = context;
        _set = _context.Set<Event>();
        _eventBusListener = eventBusListener;
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
        var returnedEvent = await _set.FindAsync(id);
        if (returnedEvent == null)
        {
            return null;
        }
        var correlationId = Guid.NewGuid().ToString();
        var packageTask = _eventBusListener.RegisterRequest(correlationId);
        var requestMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(id))
        {
            CorrelationId = correlationId,
            ReplyTo = "event-bus",
            ApplicationProperties =
            {
                { "EventType", "GetPackageForEvent" }
            }
        };
        await _eventBusListener.Sender.SendMessageAsync(requestMessage);

        try
        {
            var packages = await Task.WhenAny(packageTask, Task.Delay(TimeSpan.FromSeconds(10)))
                .ContinueWith(t =>
                {
                    if (t.Result == packageTask)
                    {
                        return packageTask.Result;
                    }
                    return [];
                });
            returnedEvent.Packages = packages;
        }
        catch (Exception ex)
        {
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
