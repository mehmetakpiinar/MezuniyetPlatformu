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
        public async Task<IActionResult> GetAktifIlanlar([FromQuery] string? s, [FromQuery] string? sehir, [FromQuery] string? tur)
        {
            try
            {
                var query = _context.JobPosts
                                    .Include(j => j.EmployerProfile)
                                    .ThenInclude(e => e.Company)
                                    .Where(j => j.IsActive == true)
                                    .AsQueryable();

                if (!string.IsNullOrEmpty(s))
                {
                    s = s.ToLower();
                    query = query.Where(j => j.Title.ToLower().Contains(s) ||
                                             j.Description.ToLower().Contains(s) ||
                                             j.EmployerProfile.Company.CompanyName.ToLower().Contains(s));
                }

                if (!string.IsNullOrEmpty(sehir))
                {
                    query = query.Where(j => j.Location.ToLower().Contains(sehir.ToLower()));
                }

                if (!string.IsNullOrEmpty(tur))
                {
                    query = query.Where(j => j.JobType == tur);
                }

                var ilanlar = await query.OrderByDescending(j => j.PublishedDate).ToListAsync();

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
                var ilan = await _context.JobPosts
                                         .Include(j => j.EmployerProfile)
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
        [Authorize]
        public async Task<IActionResult> DeleteIlan(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var ilan = await _context.JobPosts.FindAsync(id);
            if (ilan == null) return NotFound("Silinecek ilan bulunamadı.");

            bool isAdmin = User.IsInRole("Admin");

            bool isOwner = false;

            var employerProfile = await _context.EmployerProfiles.FirstOrDefaultAsync(ep => ep.UserId == userId);
            if (employerProfile != null && ilan.EmployerProfileId == employerProfile.EmployerProfileId)
            {
                isOwner = true;
            }

            if (!isAdmin && !isOwner)
            {
                return Forbid("Bu ilanı silme yetkiniz yok. Sadece ilanı açan veya Admin silebilir.");
            }

            try
            {
                var applications = await _context.JobApplications.Where(a => a.JobPostId == id).ToListAsync();
                if (applications.Any())
                {
                    _context.JobApplications.RemoveRange(applications);
                }

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
        [HttpGet("my-applications")]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var applications = await _context.JobApplications
                .Include(a => a.JobPost)
                .ThenInclude(jp => jp.EmployerProfile)
                .ThenInclude(ep => ep.Company)
                .Where(a => a.CandidateUserId == userId)
                .OrderByDescending(a => a.ApplicationDate)
                .Select(a => new
                {
                    ApplicationId = a.ApplicationId,
                    JobPostId = a.JobPostId,
                    ApplicationDate = a.ApplicationDate,
                    Status = a.Status,
                    JobPostTitle = a.JobPost.Title,
                    CompanyName = a.JobPost.EmployerProfile.Company.CompanyName,
                    CompanyLogoURL = a.JobPost.EmployerProfile.Company.LogoURL
                })
                .ToListAsync();

            return Ok(applications);
        }
        [HttpGet("my-posts")]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> GetMyJobPosts()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var employerProfile = await _context.EmployerProfiles
                                                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employerProfile == null) return BadRequest("İşveren profili bulunamadı.");

            var myPosts = await _context.JobPosts
                .Where(j => j.EmployerProfileId == employerProfile.EmployerProfileId)
                .OrderByDescending(j => j.PublishedDate)
                .ToListAsync();

            return Ok(myPosts);
        }
    }
}