using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using Microsoft.AspNetCore.Authorization;
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
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Kullanıcı ID bulunamadı.");
            var kullaniciId = int.Parse(userIdString);

            if (User.IsInRole("Isveren"))
            {
                var isverenProfil = await _context.EmployerProfiles
                    .Include(p => p.User)
                    .Include(p => p.Company)
                    .FirstOrDefaultAsync(p => p.UserId == kullaniciId);

                if (isverenProfil == null) return NotFound("İşveren profili bulunamadı.");

                return Ok(new
                {
                    profileId = isverenProfil.EmployerProfileId,
                    userId = isverenProfil.UserId,
                    user = new
                    {
                        firstName = isverenProfil.User.FirstName,
                        lastName = isverenProfil.User.LastName,
                        email = isverenProfil.User.Email
                    },
                    phoneNumber = isverenProfil.PhoneNumber,
                    universityName = isverenProfil.Company.CompanyName,
                    studyProgram = isverenProfil.Company.Sector,
                    about = isverenProfil.Company.About,
                    linkedInURL = isverenProfil.Company.Website,
                    profilePhotoURL = isverenProfil.Company.LogoURL
                });
            }

            var ogrenciProfil = await _context.AlumniProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == kullaniciId);

            if (ogrenciProfil == null) return NotFound("Öğrenci profili bulunamadı.");

            return Ok(new
            {
                profileId = ogrenciProfil.ProfileId,
                userId = ogrenciProfil.UserId,
                user = new
                {
                    firstName = ogrenciProfil.User.FirstName,
                    lastName = ogrenciProfil.User.LastName,
                    email = ogrenciProfil.User.Email
                },
                graduationYear = ogrenciProfil.GraduationYear,
                phoneNumber = ogrenciProfil.PhoneNumber,
                studyProgram = ogrenciProfil.StudyProgram,
                universityName = ogrenciProfil.UniversityName,
                skills = ogrenciProfil.Skills,
                about = ogrenciProfil.About,
                gitHubURL = ogrenciProfil.GitHubURL,
                linkedInURL = ogrenciProfil.LinkedInURL,
                profilePhotoURL = ogrenciProfil.ProfilePhotoURL
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateKullaniciProfili([FromBody] UpdateProfileDto updateDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Oturum süresi dolmuş.");

            if (!int.TryParse(userIdString, out int kullaniciId)) return BadRequest("Geçersiz Kullanıcı ID.");

            try
            {
                var user = await _context.Users.FindAsync(kullaniciId);

                if (User.IsInRole("Isveren"))
                {
                    var isveren = await _context.EmployerProfiles
                                                .Include(e => e.Company)
                                                .FirstOrDefaultAsync(p => p.UserId == kullaniciId);

                    if (isveren == null) return NotFound("İşveren kaydı bulunamadı.");

                    isveren.Company.CompanyName = updateDto.UniversityName ?? "";
                    isveren.Company.Sector = updateDto.StudyProgram ?? "";
                    isveren.Company.About = updateDto.About ?? "";
                    isveren.Company.LogoURL = updateDto.ProfilePhotoURL ?? "";
                    isveren.Company.Website = updateDto.LinkedInURL ?? "";
                    isveren.PhoneNumber = updateDto.PhoneNumber ?? "";

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        Success = true,
                        Message = "İşveren profili güncellendi.",
                        CompanyName = isveren.Company.CompanyName
                    });
                }

                var profil = await _context.AlumniProfiles.FirstOrDefaultAsync(p => p.UserId == kullaniciId);

                if (profil == null) return NotFound("Öğrenci profili bulunamadı.");

                profil.ProfilePhotoURL = updateDto.ProfilePhotoURL ?? "";
                profil.About = updateDto.About ?? "";
                profil.GraduationYear = updateDto.GraduationYear ?? 0;
                profil.StudyProgram = updateDto.StudyProgram ?? "";
                profil.LinkedInURL = updateDto.LinkedInURL ?? "";
                profil.GitHubURL = updateDto.GitHubURL ?? "";
                profil.PhoneNumber = updateDto.PhoneNumber ?? "";
                profil.Skills = updateDto.Skills ?? "";
                profil.UniversityName = updateDto.UniversityName ?? "";

                if (!string.IsNullOrEmpty(updateDto.UniversityName))
                {
                    var university = await _context.Universities
                        .FirstOrDefaultAsync(u => u.Name == updateDto.UniversityName);

                    if (university != null && user != null)
                    {
                        user.UniversityId = university.UniversityId;
                        _context.Entry(user).State = EntityState.Modified;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Güncelleme Başarılı",
                    Data = new
                    {
                        profil.ProfileId,
                        profil.UserId,
                        profil.UniversityName,
                        profil.StudyProgram,
                        profil.About,
                        profil.ProfilePhotoURL,
                        profil.PhoneNumber,
                        profil.Skills,
                        profil.GitHubURL,
                        profil.LinkedInURL,
                        profil.GraduationYear
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu Hatası: {ex.Message}");
            }
        }
    }
}