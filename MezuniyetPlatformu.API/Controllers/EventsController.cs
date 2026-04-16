using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using MezuniyetPlatformu.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public EventsController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var events = await _context.Events
                                     .Where(e => e.IsActive)
                                     .OrderByDescending(e => e.EventDate)
                                     .ToListAsync();

            var eventDtos = events.Select(e => new EventDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                UniversityId = e.UniversityId,
                ImageUrl = e.ImageUrl
            }).ToList();

            return Ok(eventDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            if (!string.IsNullOrEmpty(userIdString)) int.TryParse(userIdString, out currentUserId);

            var ev = await _context.Events.FindAsync(id);

            if (ev == null) return NotFound();

            var eventDto = new EventDto
            {
                EventId = ev.EventId,
                Title = ev.Title,
                Description = ev.Description,
                EventDate = ev.EventDate,
                Location = ev.Location,
                UniversityId = ev.UniversityId,
                ImageUrl = ev.ImageUrl,

                IsJoined = await _context.EventParticipants
                                         .AnyAsync(ep => ep.EventId == id && ep.UserId == currentUserId)
            };

            return Ok(eventDto);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(EventDto eventDto)
        {
            var newEvent = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                EventDate = eventDto.EventDate,
                Location = eventDto.Location,
                UniversityId = eventDto.UniversityId,
                IsActive = true,
                ImageUrl = eventDto.ImageUrl
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Etkinlik oluşturuldu", id = newEvent.EventId });
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> ToggleJoin(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var existing = await _context.EventParticipants
                                         .FirstOrDefaultAsync(ep => ep.EventId == id && ep.UserId == userId);

            if (existing != null)
            {
                _context.EventParticipants.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Katılım iptal edildi", isJoined = false });
            }
            else
            {
                var participant = new EventParticipant { EventId = id, UserId = userId, ParticipationDate = DateTime.Now };
                _context.EventParticipants.Add(participant);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Katılım sağlandı", isJoined = true });
            }
        }
    }
}