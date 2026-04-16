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
    public class DashboardController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public DashboardController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Isveren")
            {
                var employerProfile = await _context.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
                if (employerProfile == null) return Ok(new { });

                var totalPosts = await _context.JobPosts.CountAsync(x => x.EmployerProfileId == employerProfile.EmployerProfileId);

                var activePosts = await _context.JobPosts.CountAsync(x => x.EmployerProfileId == employerProfile.EmployerProfileId && x.IsActive);

                var totalApplications = await _context.JobApplications
                                              .Include(a => a.JobPost)
                                              .CountAsync(a => a.JobPost.EmployerProfileId == employerProfile.EmployerProfileId);

                return Ok(new
                {
                    Role = "Isveren",
                    TotalPosts = totalPosts,
                    ActivePosts = activePosts,
                    TotalApplications = totalApplications
                });
            }
            else
            {
                var myApplications = await _context.JobApplications.CountAsync(x => x.CandidateUserId == userId);

                var totalOpportunities = await _context.JobPosts.CountAsync(x => x.IsActive);

                var unreadMessages = await _context.Messages.CountAsync(m => m.RecipientUserId == userId && !m.IsRead);

                return Ok(new
                {
                    Role = "Ogrenci",
                    MyApplications = myApplications,
                    TotalOpportunities = totalOpportunities,
                    UnreadMessages = unreadMessages
                });
            }
        }
    }
}