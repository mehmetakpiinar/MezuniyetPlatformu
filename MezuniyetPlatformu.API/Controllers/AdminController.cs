using MezuniyetPlatformu.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public AdminController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobPosts = await _context.JobPosts.CountAsync(),
                ActiveJobPosts = await _context.JobPosts.CountAsync(x => x.IsActive),
                TotalExperiencePosts = await _context.ExperiencePosts.CountAsync()
            };
            return Ok(stats);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.TypeName)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    Role = u.TypeName.TypeName,
                    u.RegisterTime
                })
                .OrderByDescending(u => u.RegisterTime)
                .ToListAsync();
            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var messages = await _context.Messages
                .Where(m => m.SenderUserId == id || m.RecipientUserId == id)
                .ToListAsync();
            _context.Messages.RemoveRange(messages);

            var expPosts = await _context.ExperiencePosts.Where(e => e.AuthorUserId == id).ToListAsync();
            _context.ExperiencePosts.RemoveRange(expPosts);

            var applications = await _context.JobApplications.Where(a => a.CandidateUserId == id).ToListAsync();
            _context.JobApplications.RemoveRange(applications);

            var employerProfile = await _context.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == id);
            if (employerProfile != null)
            {
                var jobPosts = await _context.JobPosts.Where(j => j.EmployerProfileId == employerProfile.EmployerProfileId).ToListAsync();
                foreach (var job in jobPosts)
                {
                    var jobApps = await _context.JobApplications.Where(ja => ja.JobPostId == job.JobPostId).ToListAsync();
                    _context.JobApplications.RemoveRange(jobApps);
                }
                _context.JobPosts.RemoveRange(jobPosts);
                _context.EmployerProfiles.Remove(employerProfile);
            }

            var alumniProfile = await _context.AlumniProfiles.FirstOrDefaultAsync(a => a.UserId == id);
            if (alumniProfile != null) _context.AlumniProfiles.Remove(alumniProfile);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("jobposts/{id}")]
        public async Task<IActionResult> DeleteJobPost(int id)
        {
            var post = await _context.JobPosts.FindAsync(id);
            if (post == null) return NotFound("İlan bulunamadı.");

            var apps = await _context.JobApplications.Where(a => a.JobPostId == id).ToListAsync();
            _context.JobApplications.RemoveRange(apps);

            _context.JobPosts.Remove(post);

            var result = await _context.SaveChangesAsync();

            if (result > 0) return Ok(new { message = "İlan başarıyla silindi." });
            return BadRequest("İlan veritabanından silinemedi.");
        }

        [HttpDelete("experienceposts/{id}")]
        public async Task<IActionResult> DeleteExperiencePost(int id)
        {
            var post = await _context.ExperiencePosts.FindAsync(id);
            if (post == null)
            {
                return NotFound($"ID'si {id} olan paylaşım bulunamadı.");
            }

            try
            {
                var likes = await _context.ExperiencePostLikes.Where(l => l.PostId == id).ToListAsync();
                if (likes.Any())
                {
                    _context.ExperiencePostLikes.RemoveRange(likes);
                }

                var comments = await _context.ExperiencePostComments.Where(c => c.PostId == id).ToListAsync();
                if (comments.Any())
                {
                    _context.ExperiencePostComments.RemoveRange(comments);
                }

                _context.ExperiencePosts.Remove(post);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Paylaşım, yorumlar ve beğeniler başarıyla silindi." });
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return StatusCode(500, $"Veritabanı Hatası: {innerError}");
            }
        }

        [HttpGet("universities")]
        public async Task<IActionResult> GetUniversities()
        {
            var universities = await _context.Universities
                .Select(u => new
                {
                    u.UniversityId,
                    u.Name,
                    u.LogoUrl,
                    StudentCount = _context.Users.Count(user => user.UniversityId == u.UniversityId && (user.UserTypeId == 1 || user.UserTypeId == 2))
                })
                .ToListAsync();

            return Ok(universities);
        }

        [HttpPost("universities")]
        public async Task<IActionResult> CreateUniversity([FromBody] UniversityCreateDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name)) return BadRequest("Üniversite adı boş olamaz.");

            var newUni = new MezuniyetPlatformu.Entities.University
            {
                Name = dto.Name,
                LogoUrl = dto.LogoUrl
            };

            _context.Universities.Add(newUni);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Üniversite eklendi." });
        }

        [HttpDelete("universities/{id}")]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var uni = await _context.Universities.FindAsync(id);
            if (uni == null) return NotFound("Üniversite bulunamadı.");

            bool hasUsers = await _context.Users.AnyAsync(u => u.UniversityId == id);

            if (hasUsers)
            {
                return BadRequest($"Bu üniversiteye kayıtlı kullanıcılar var. Önce kullanıcıları silmelisiniz.");
            }

            _context.Universities.Remove(uni);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Üniversite silindi." });
        }

        [HttpGet("jobposts")]
        public async Task<IActionResult> GetAllJobPosts()
        {
            var posts = await _context.JobPosts
                .Include(j => j.EmployerProfile)
                .ThenInclude(e => e.User)
                .OrderByDescending(j => j.CreatedDate)
                .Select(j => new
                {
                    j.JobPostId,
                    j.Title,

                    CompanyName = j.EmployerProfile != null
                                  ? (j.EmployerProfile.CompanyName ?? "Şirket Adı Yok")
                                  : "Silinmiş Şirket",

                    EmployerName = (j.EmployerProfile != null && j.EmployerProfile.User != null)
                                   ? j.EmployerProfile.User.FirstName + " " + j.EmployerProfile.User.LastName
                                   : "Silinmiş Kullanıcı",

                    CreatedDate = j.CreatedDate != null ? j.CreatedDate : DateTime.Now,

                    j.IsActive
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("experienceposts")]
        public async Task<IActionResult> GetAllExperiencePosts()
        {
            var posts = await _context.ExperiencePosts
                .Include(e => e.AuthorUser)
                .Select(e => new
                {
                    e.PostId,
                    e.Title,
                    AuthorName = e.AuthorUser != null
                                 ? e.AuthorUser.FirstName + " " + e.AuthorUser.LastName
                                 : "Anonim / Silinmiş",
                    e.CreatedDate
                })
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return Ok(posts);
        }
    }

    public class UniversityCreateDto
    {
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
    }
}   