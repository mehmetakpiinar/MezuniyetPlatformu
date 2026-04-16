using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MezuniyetPlatformu.Entities;

namespace MezuniyetPlatformu.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var universities = new List<University>();

            try
            {
                universities = await client.GetFromJsonAsync<List<University>>("Universities");
            }
            catch
            {
                universities = new List<University>
                 {
                     new University { UniversityId = 1, Name = "Fırat Üniversitesi" },
                     new University { UniversityId = 2, Name = "Adıyaman Üniversitesi" }
                 };
            }

            ViewBag.Universities = new SelectList(universities, "UniversityId", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var clientForList = _httpClientFactory.CreateClient("ApiClient");
                var universities = new List<University>();
                try { universities = await clientForList.GetFromJsonAsync<List<University>>("Universities"); }
                catch
                {
                    universities = new List<University> {
                         new University { UniversityId = 1, Name = "Fırat Üniversitesi" },
                         new University { UniversityId = 2, Name = "Adıyaman Üniversitesi" }
                     };
                }
                ViewBag.Universities = new SelectList(universities, "UniversityId", "Name");

                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var apiRequestData = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                UserTypeId = model.UserTypeId,
                UniversityId = model.UniversityId
            };

            var content = new StringContent(JsonSerializer.Serialize(apiRequestData), Encoding.UTF8, "application/json");

            _logger.LogInformation("API'ye kayıt isteği gönderiliyor: {Email}", model.Email);
            var response = await client.PostAsync("auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("API'den başarılı kayıt cevabı alındı: {Email}", model.Email);
                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı! Lütfen giriş yapın.";
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API'den kayıt hatası alındı: {StatusCode} - {Error}", response.StatusCode, errorContent);
                ModelState.AddModelError(string.Empty, "Kayıt başarısız: " + errorContent);

                var universities = new List<University>();
                try { universities = await client.GetFromJsonAsync<List<University>>("Universities"); }
                catch
                {
                    universities = new List<University> {
                         new University { UniversityId = 1, Name = "Fırat Üniversitesi" },
                         new University { UniversityId = 2, Name = "Adıyaman Üniversitesi" }
                     };
                }
                ViewBag.Universities = new SelectList(universities, "UniversityId", "Name");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("ApiClient");

            var apiRequestData = new
            {
                Email = model.Email,
                Sifre = model.Sifre
            };

            var content = new StringContent(JsonSerializer.Serialize(apiRequestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                var token = tokenObject.GetProperty("token").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError(string.Empty, "API'den geçerli bir token alınamadı.");
                    return View(model);
                }

                _logger.LogInformation("Kullanıcı {Email} başarıyla giriş yaptı.", model.Email);
                await SignInUser(token);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var userRole = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;

                if (userRole == "Isveren")
                {
                    return RedirectToAction("MyPosts", "JobPosts");
                }
                else if (userRole == "UniversityRep")
                {
                    return RedirectToAction("Dashboard", "University");
                }
                else if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API giriş hatası: {Error}", errorContent);
                ModelState.AddModelError(string.Empty, "Giriş başarısız: " + errorContent);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Kullanıcı oturumu kapattı.");
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();
            claims.Add(new Claim("jwt", token));

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Email,
                "role"
            );

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = jwtToken.ValidTo
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}