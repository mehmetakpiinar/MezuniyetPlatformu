using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using Microsoft.AspNetCore.Authorization; 

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostsController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public JobPostsController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAktifIlanlar()
        {
            try
            {
                var ilanlar = await _context.JobPosts.Include(j => j.EmployerProfile)
                                            .Where(ilan => ilan.IsActive == true)
                                            .OrderByDescending(ilan => ilan.PublishedDate)
                                            .ToListAsync();
                return Ok(ilanlar);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIlanDetayi(int id)
        {
            try
            {
                // ---- DÜZELTİLMİŞ SORGU ----
                // Sadece ilanı değil, ilanın sahibini (EmployerProfile) de getiriyoruz
                var ilan = await _context.JobPosts
                                         .Include(j => j.EmployerProfile) // <-- BU SATIR EKSİKTİ!
                                         .FirstOrDefaultAsync(i => i.JobPostId == id);

                if (ilan == null)
                {
                    return NotFound("İlan bulunamadı.");
                }

                return Ok(ilan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> CreateIlan([FromBody] CreateJobPostingDto ilanDto)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized("Token'da kullanıcı ID'si bulunamadı.");
            }
            var kullaniciId = int.Parse(kullaniciIdString);

            var isverenProfili = await _context.EmployerProfiles
                                             .Include(p => p.Company)
                                             .FirstOrDefaultAsync(ip => ip.UserId == kullaniciId);

            if (isverenProfili == null || isverenProfili.Company == null)
            {
                return BadRequest("İlan oluşturabilmek için bir şirket profiline sahip olmalısınız.");
            }

            var yeniIlan = new JobPost
            {
                EmployerProfileId = isverenProfili.EmployerProfileId,
                CompanyId = isverenProfili.CompanyId,
                Title = ilanDto.Title,
                Description = ilanDto.Description,
                Location = ilanDto.Location,
                JobType = ilanDto.JobType,
                Deadline = ilanDto.Deadline,
                PublishedDate = DateTime.Now,
                IsActive = true
            };

            try
            {
                await _context.JobPosts.AddAsync(yeniIlan);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetIlanDetayi), new { id = yeniIlan.JobPostId }, yeniIlan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> UpdateIlan(int id, [FromBody] UpdateJobPostingDto ilanDto)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized();
            }
            var kullaniciId = int.Parse(kullaniciIdString);

            var isvereninProfili = await _context.EmployerProfiles.FirstOrDefaultAsync(ip => ip.UserId == kullaniciId);

            if (isvereninProfili == null)
            {
                return Forbid("İşlem yapmak için geçerli bir işveren profiliniz bulunamadı.");
            }

            var ilan = await _context.JobPosts.FindAsync(id);
            if (ilan == null)
            {
                return NotFound("Güncellenecek ilan bulunamadı.");
            }

            if (ilan.EmployerProfileId != isvereninProfili.EmployerProfileId)
            {
                return Forbid("Bu ilanı güncelleme yetkiniz yok.");
            }

            ilan.Title = ilanDto.Title;
            ilan.Description = ilanDto.Description;
            ilan.Location = ilanDto.Location;
            ilan.JobType = ilanDto.JobType;
            ilan.Deadline = ilanDto.Deadline;
            ilan.IsActive = ilanDto.IsActive;

            try
            {
                _context.JobPosts.Update(ilan);
                await _context.SaveChangesAsync();

                return Ok(ilan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> DeleteIlan(int id)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized();
            }
            var kullaniciId = int.Parse(kullaniciIdString);

            var isvereninProfili = await _context.EmployerProfiles.FirstOrDefaultAsync(ip => ip.UserId == kullaniciId);

            if (isvereninProfili == null)
            {
                return Forbid("İşlem yapmak için geçerli bir işveren profiliniz bulunamadı.");
            }

            var ilan = await _context.JobPosts.FindAsync(id);

            if (ilan == null)
            {
                return NotFound("Silinecek ilan bulunamadı.");
            }

            if (ilan.EmployerProfileId != isvereninProfili.EmployerProfileId)
            {
                return Forbid("Bu ilanı silme yetkiniz yok.");
            }
            try
            {
                _context.JobPosts.Remove(ilan);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> ApplyToJob(int id)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized(); 
            }
            var adayKullaniciId = int.Parse(kullaniciIdString);

            var ilan = await _context.JobPosts.FindAsync(id);

            if (ilan == null || ilan.IsActive == false)
            {
                return NotFound("Başvuruya açık bir ilan bulunamadı."); 
            }
            bool zatenBasvurmus = await _context.JobApplications
                                            .AnyAsync(b => b.JobPostId == id && b.CandidateUserId == adayKullaniciId);

            if (zatenBasvurmus)
            {
                return BadRequest("Bu ilana zaten başvurmuşsunuz.");
            }
            var yeniBasvuru = new JobApplication
            {
                JobPostId = id,
                CandidateUserId = adayKullaniciId,
                ApplicationDate = DateTime.UtcNow,
                Status = "Inceleniyor"
            };

            try
            {
                await _context.JobApplications.AddAsync(yeniBasvuru);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetIlanDetayi), new { id = yeniBasvuru.JobPostId }, yeniBasvuru);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}/applications")]
        [Authorize(Roles = "Isveren")] 
        public async Task<IActionResult> GetJobApplications(int id)
        {
            var kullaniciIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(kullaniciIdString))
            {
                return Unauthorized();
            }
            var isverenKullaniciId = int.Parse(kullaniciIdString);

            var isvereninProfili = await _context.EmployerProfiles.FirstOrDefaultAsync(ip => ip.UserId == isverenKullaniciId);
            if (isvereninProfili == null)
            {
                return Forbid("İşlem yapmak için geçerli bir işveren profiliniz bulunamadı.");
            }
            var ilan = await _context.JobPosts.FindAsync(id);

            if (ilan == null)
            {
                return NotFound("İlan bulunamadı.");
            }
            if (ilan.EmployerProfileId != isvereninProfili.EmployerProfileId)
            {
                return StatusCode(403, "Bu ilana ait başvuruları görme yetkiniz yok.");
            }

            try
            {
                var basvurular = await _context.JobApplications
                    .Where(b => b.JobPostId == id)
                    .Include(b => b.CandidateUser)
                    .OrderByDescending(b => b.ApplicationDate) 
                    .ToListAsync();

                return Ok(basvurular);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}