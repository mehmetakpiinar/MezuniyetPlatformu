using MezuniyetPlatformu.Business;
using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IHttpClientFactory httpClientFactory, ILogger<ProfileController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("Profiller/me");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Null) return View(new ProfileViewModel());

                    var model = new ProfileViewModel
                    {
                        User = new UserViewModel
                        {
                            FirstName = root.TryGetProperty("user", out var u) ? GetJsonString(u, "firstName") : "",
                            LastName = root.TryGetProperty("user", out var u2) ? GetJsonString(u2, "lastName") : "",
                            Email = root.TryGetProperty("user", out var u3) ? GetJsonString(u3, "email") : ""
                        },

                        GraduationYear = GetJsonInt(root, "graduationYear"),

                        PhoneNumber = GetJsonString(root, "phoneNumber"),

                        StudyProgram = GetJsonString(root, "studyProgram"),
                        About = GetJsonString(root, "about"),

                        UniversityName = GetJsonString(root, "universityName"),
                        Skills = GetJsonString(root, "skills"),

                        GitHubURL = GetJsonString(root, "gitHubURL"),
                        LinkedInURL = GetJsonString(root, "linkedInURL"),
                        ProfilePhotoURL = GetJsonString(root, "profilePhotoURL"),

                        ProfileId = GetJsonInt(root, "profileId"),
                        UserId = GetJsonInt(root, "userId")
                    };

                    return View(model);
                }
            }

            return View(new ProfileViewModel());
        }


        private string GetJsonString(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null) return "-";

            var prop = element.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

            if (prop.Value.ValueKind == JsonValueKind.String)
                return prop.Value.GetString();

            return "";
        }

        private int GetJsonInt(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null) return 0;

            var prop = element.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

            if (prop.Value.ValueKind == JsonValueKind.Number)
                return prop.Value.GetInt32();

            return 0;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("Profiller/me");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userIdInt);

            var model = new ProfileViewModel { UserId = userIdInt };

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var root = doc.RootElement;
                    if (root.ValueKind != JsonValueKind.Null)
                    {
                        model.ProfileId = GetJsonInt(root, "profileId");
                        model.GraduationYear = GetJsonInt(root, "graduationYear");
                        model.StudyProgram = GetJsonString(root, "studyProgram"); 
                        model.UniversityName = GetJsonString(root, "universityName");
                        model.Skills = GetJsonString(root, "skills");
                        model.About = GetJsonString(root, "about");
                        model.PhoneNumber = GetJsonString(root, "phoneNumber");
                        model.GitHubURL = GetJsonString(root, "gitHubURL");
                        model.LinkedInURL = GetJsonString(root, "linkedInURL");
                        model.ProfilePhotoURL = GetJsonString(root, "profilePhotoURL");
                    }
                }
            }

            if (User.IsInRole("Isveren"))
            {
                return View("EditEmployer", model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (model.ResimDosyasi != null && model.ResimDosyasi.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ResimDosyasi.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResimDosyasi.CopyToAsync(fileStream);
                }
                model.ProfilePhotoURL = "/uploads/" + uniqueFileName;
            }

            var updateData = new
            {
                ProfilePhotoURL = model.ProfilePhotoURL,
                About = model.About,

                GraduationYear = model.GraduationYear ?? 0,

                StudyProgram = model.StudyProgram,
                LinkedInURL = model.LinkedInURL,
                GitHubURL = model.GitHubURL,
                PhoneNumber = model.PhoneNumber,
                UniversityName = model.UniversityName,
                Skills = model.Skills
            };

            var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");

            var response = await client.PutAsync("Profiller/me", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(errorMsg)) errorMsg = "Sunucuya ulaşılamadı (Adres Hatası olabilir).";

                TempData["ErrorMessage"] = "Güncelleme hatası: " + errorMsg;
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateAiAbout([FromBody] ProfileViewModel model)
        {
            try
            {
                var aiService = HttpContext.RequestServices.GetRequiredService<AiService>();
                var result = await aiService.GenerateAboutMeAsync(model.StudyProgram, model.UniversityName, model.Skills);

                if (result.StartsWith("API_ERROR") || result.StartsWith("SYSTEM_ERROR"))
                {
                    return Json(new { success = false, text = result });
                }

                return Json(new { success = true, text = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, text = ex.Message });
            }
        }
    }
}