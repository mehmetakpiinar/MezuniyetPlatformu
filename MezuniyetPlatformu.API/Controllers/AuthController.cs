using MezuniyetPlatformu.API.DTOs;
using MezuniyetPlatformu.DataAccess;
using MezuniyetPlatformu.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MezuniyetPlatformu.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MezuniyetPlatformuDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MezuniyetPlatformuDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(p => p.Email == registerDto.Email))
            {
                return BadRequest("Bu e-posta adresi zaten kullanılmaktadır.");
            }

            string sifreHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var yeniKullanici = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = sifreHash,
                UserTypeId = registerDto.UserTypeId,
                RegisterTime = DateTime.Now,
                UniversityId = registerDto.UniversityId
            };

            await _context.Users.AddAsync(yeniKullanici);
            await _context.SaveChangesAsync();

            if (yeniKullanici.UserTypeId == 1 || yeniKullanici.UserTypeId == 2)
            {
                var yeniProfil = new AlumniProfile
                {
                    UserId = yeniKullanici.UserId
                };
                await _context.AlumniProfiles.AddAsync(yeniProfil);
            }
            else if (yeniKullanici.UserTypeId == 3)
            {
                var yeniSirket = new Company
                {
                    CompanyName = yeniKullanici.FirstName + " " + yeniKullanici.LastName + "'in Şirketi",
                };
                await _context.Companies.AddAsync(yeniSirket);
                await _context.SaveChangesAsync();

                var yeniIsverenProfili = new EmployerProfile
                {
                    UserId = yeniKullanici.UserId,
                    CompanyId = yeniSirket.CompanyId,
                    Position = "Yönetici"
                };
                await _context.EmployerProfiles.AddAsync(yeniIsverenProfili);
            }
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                                     .Include(u => u.TypeName)
                                     .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Sifre, user.Password))
            {
                return Unauthorized("Geçersiz e-posta veya şifre.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.TypeName.TypeName)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}