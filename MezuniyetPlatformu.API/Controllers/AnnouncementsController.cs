using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using MezuniyetPlatformu.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public AnnouncementsController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet("student-list")]
        public async Task<IActionResult> GetAnnouncementsForStudent()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var student = await _context.Users.FindAsync(userId);
            if (student == null || student.UniversityId == null)
                return BadRequest("Bir üniversiteye kayıtlı değilsiniz.");

            var announcements = await _context.Announcements
                .Where(a => a.UniversityId == student.UniversityId)
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new
                {
                    a.AnnouncementId,
                    a.Title,
                    a.Message,
                    a.CreatedDate,
                    a.UniversityId
                })
                .ToListAsync();

            return Ok(announcements);
        }


        [HttpPost("create")]
        [Authorize(Roles = "UniversityRep")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] AnnouncementCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(userId);
            if (repUser?.UniversityId == null) return BadRequest("Üniversite kaydı bulunamadı.");

            var announcement = new Announcement
            {
                Title = dto.Title,
                Message = dto.Message,
                CreatedDate = DateTime.Now,
                UniversityId = repUser.UniversityId.Value
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyuru başarıyla kaydedildi." });
        }

        [HttpGet("my-announcements")]
        [Authorize(Roles = "UniversityRep")]
        public async Task<IActionResult> GetMyAnnouncements()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(userId);
            if (repUser?.UniversityId == null) return BadRequest("Üniversite kaydı yok.");

            var announcements = await _context.Announcements
                .Where(a => a.UniversityId == repUser.UniversityId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return Ok(announcements);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "UniversityRep")] 
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(userId);
            if (repUser?.UniversityId == null) return BadRequest("Yetkiniz yok.");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound("Duyuru bulunamadı.");

            if (announcement.UniversityId != repUser.UniversityId)
                return StatusCode(403, "Bu duyuruyu silme yetkiniz yok.");

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyuru silindi." });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "UniversityRep")]
        public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] AnnouncementCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(userId);
            if (repUser?.UniversityId == null) return BadRequest("Yetkiniz yok.");

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound("Duyuru bulunamadı.");

            if (announcement.UniversityId != repUser.UniversityId)
                return StatusCode(403, "Bu duyuruyu düzenleme yetkiniz yok.");

            announcement.Title = dto.Title;
            announcement.Message = dto.Message;

            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyuru güncellendi." });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "UniversityRep")]
        public async Task<IActionResult> GetAnnouncementById(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();
            return Ok(announcement);
        }
    }
}