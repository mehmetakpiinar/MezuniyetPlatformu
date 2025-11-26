using MezuniyetPlatformu.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class UserTypesController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;

        public UserTypesController(MezuniyetPlatformuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetKullaniciTipleri()
        {
            var userTypes = await _context.UserTypes.ToListAsync();
            return Ok(userTypes);
        }
    }
}
