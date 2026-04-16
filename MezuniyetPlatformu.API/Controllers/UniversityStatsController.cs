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
    [Authorize(Roles = "UniversityRep")]
    public class UniversityStatsController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public UniversityStatsController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (repUser?.UniversityId == null) return BadRequest("Üniversite kaydı bulunamadı.");

            var uniId = repUser.UniversityId;


            var totalStudents = await _context.Users
                .Include(u => u.TypeName)
                .CountAsync(u => u.UniversityId == uniId && u.TypeName.TypeName == "Ogrenci");

            var employedCount = await _context.ExperiencePosts
                .Where(e => e.AuthorUser.UniversityId == uniId)
                .Select(e => e.AuthorUserId)
                .Distinct()
                .CountAsync();

            var dto = new UniversityDashboardDto
            {
                UniversityName = repUser.University.Name,
                UniversityLogo = repUser.University.LogoUrl,
                TotalStudents = totalStudents,
                EmployedGraduates = employedCount,
                JobSeekers = totalStudents - employedCount, // Basit mantık

                SoftwareSectorCount = 45,
                FinanceSectorCount = 20,
                EngineeringSectorCount = 35
            };

            return Ok(dto);
        }
        [HttpGet("students")]
        public async Task<IActionResult> GetMyStudents()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(userId);
            if (repUser?.UniversityId == null) return BadRequest("Üniversite bulunamadı.");

            var students = await _context.Users
                .Include(u => u.TypeName)
                .Where(u => u.UniversityId == repUser.UniversityId && u.TypeName.TypeName == "Ogrenci")
                .Select(u => new StudentDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    RegisterTime = u.RegisterTime
                })
                .ToListAsync();

            return Ok(students);
        }
        [HttpGet("student-detail/{studentId}")]
        public async Task<IActionResult> GetStudentDetail(int studentId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var repUserId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(repUserId);
            if (repUser == null || repUser.UniversityId == null)
            {
                return BadRequest("İşlem yapan kullanıcının üniversite kaydı yok.");
            }

            var student = await _context.Users
                .Include(u => u.TypeName)
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.UserId == studentId);

            if (student == null)
            {
                return NotFound("Öğrenci bulunamadı.");
            }

            if (student.UniversityId != repUser.UniversityId)
            {
                return StatusCode(403, "Bu öğrenci sizin üniversitenize kayıtlı değil.");
            }

            var experiences = await _context.ExperiencePosts
                .Where(e => e.AuthorUserId == studentId)
                .OrderByDescending(e => e.CreatedDate)
                .Select(e => new
                {
                    e.PostId,
                    e.Title,
                    e.Content,
                    e.ImageUrl,
                    e.CreatedDate
                })
                .ToListAsync();
            var result = new
            {
                student.UserId,
                student.FirstName,
                student.LastName,
                FullName = $"{student.FirstName} {student.LastName}",
                student.Email,
                student.RegisterTime,

                UniversityName = student.University != null ? student.University.Name : "Belirtilmemiş",

                PhoneNumber = "Belirtilmemiş",
                Department = "Belirtilmemiş",

                Educations = new List<object>(),

                Experiences = experiences,

                Skills = new List<string>()
            };

            return Ok(result);
        }


        [HttpPost("ApproveGraduation/{studentId}")]
        public async Task<IActionResult> ApproveGraduation(int studentId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Oturum bulunamadı.");
            var repUserId = int.Parse(userIdString);

            var repUser = await _context.Users.FindAsync(repUserId);
            if (repUser == null || repUser.UniversityId == null)
                return BadRequest("İşlem yapan kullanıcının üniversite kaydı yok.");

            var student = await _context.Users.FindAsync(studentId);
            if (student == null) return NotFound("Öğrenci bulunamadı.");

            if (student.UniversityId != repUser.UniversityId)
                return BadRequest("Bu öğrenci sizin üniversitenize kayıtlı değil.");

            student.UserTypeId = 2;

            _context.Users.Update(student);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Öğrenci başarıyla mezun edildi." });
        }
    }
}