using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfillerController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public ProfillerController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetKullaniciProfili()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized("Kullanıcı ID'si token'da bulunamadı.");
            }

            var kullaniciId = int.Parse(userIdString);

            var profil = await _context.AlumniProfiles
                                       .Include(p => p.User)
                                       .FirstOrDefaultAsync(p => p.UserId == kullaniciId);

            if (profil == null)
                return NotFound("Bu kullanıcıya ait bir profil bulunamadı.");

            return Ok(profil);
        }


        [HttpPut("me")]
        public async Task<IActionResult> UpdateKullaniciProfili([FromBody] UpdateProfileDto updateDto)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized();
            }
            var kullaniciId = int.Parse(kullaniciIdString);

            var profil = await _context.AlumniProfiles.FirstOrDefaultAsync(p => p.UserId == kullaniciId);

            if (profil == null)
            {
                return NotFound("Güncellenecek profil bulunamadı.");
            }

            profil.ProfilePhotoURL = updateDto.ProfilePhotoURL;
            profil.About = updateDto.About;
            profil.GraduationYear = updateDto.GraduationYear;
            profil.StudyProgram = updateDto.StudyProgram;
            profil.LinkedInURL = updateDto.LinkedInURL;
            profil.GitHubURL = updateDto.GitHubURL;
            profil.PhoneNumber = updateDto.PhoneNumber;
            await _context.SaveChangesAsync();

            return Ok(profil);
        }



    }
}