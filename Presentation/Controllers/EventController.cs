using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Presentation.Factory;
using Presentation.Models;
using Presentation.Services;

namespace Presentation.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController(EventService eventService) : ControllerBase
    {
        private readonly EventService _eventService = eventService;

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            if (!events.Success)
                return BadRequest(events.Error);

            return Ok(events.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventInfoById(string id)
        {
            var eventItem = await _eventService.GetEventInfoByIdAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }
            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventRegistrationDto newEvent)
        {
                if (newEvent == null)
            {
                return BadRequest("Event cannot be null");
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _eventService.AddEventAsync(newEvent);
            if (response.Success && response.Data != null)
            {
                return CreatedAtAction(nameof(GetEventInfoById), new { id = response.Data.Id }, response.Data);
            }

            return BadRequest(response.Error);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] Event updatedEvent)
        {
            if (updatedEvent == null || id != updatedEvent.Id)
            {
                return BadRequest("Event ID mismatch");
            }
            var existingEvent = await _eventService.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }
            await _eventService.UpdateEventAsync(updatedEvent);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var existingEvent = await _eventService.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
