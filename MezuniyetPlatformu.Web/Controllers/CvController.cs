using Microsoft.AspNetCore.Mvc;
using MezuniyetPlatformu.Web.ViewModels;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    public class CvController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CvController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var model = new CvViewModel
            {
                FullName = "Veri Alınamadı",
                Email = "-",
                Phone = "-",
                University = "-",
                Department = "-",
                Skills = "",
                Experiences = new List<ExperienceDto>()
            };

            var profileResponse = await client.GetAsync("Profiller/me");

            if (profileResponse.IsSuccessStatusCode)
            {
                var jsonString = await profileResponse.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    var root = doc.RootElement;

                    if (root.ValueKind != JsonValueKind.Null)
                    {
                        if (root.TryGetProperty("user", out var userJson))
                        {
                            string ad = GetJsonString(userJson, "firstName");
                            string soyad = GetJsonString(userJson, "lastName");

                            model.FullName = $"{ad} {soyad}";

                            model.Email = GetJsonString(userJson, "email");
                        }
                        else
                        {
                            model.FullName = User.Identity.Name ?? "İsimsiz";
                            model.Email = User.FindFirstValue(ClaimTypes.Email);
                        }

                        model.University = GetJsonString(root, "universityName");
                        model.Department = GetJsonString(root, "studyProgram");
                        model.Bio = GetJsonString(root, "about");
                        model.Phone = GetJsonString(root, "phoneNumber"); 
                        model.Skills = GetJsonString(root, "skills");
                        model.GraduationYear = GetJsonInt(root, "graduationYear");

                        model.Address = "Türkiye"; 
                    }
                }
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expResponse = await client.GetAsync($"experienceposts/user/{userIdString}");

            if (expResponse.IsSuccessStatusCode)
            {
                var jsonExp = await expResponse.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                try
                {
                    var exps = JsonSerializer.Deserialize<List<ExperienceDto>>(jsonExp, options);
                    if (exps != null) model.Experiences = exps;
                }
                catch { }
            }

            return View(model);
        }
        private string GetJsonString(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null) return "-";

            
            var prop = element.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

            if (prop.Value.ValueKind == JsonValueKind.Undefined || prop.Value.ValueKind == JsonValueKind.Null) return "-";

            return (prop.Value.ValueKind == JsonValueKind.String) ? prop.Value.GetString() : "-";
        }

        private int GetJsonInt(JsonElement element, string propName)
        {
            if (element.ValueKind == JsonValueKind.Undefined || element.ValueKind == JsonValueKind.Null) return 0;

            var prop = element.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase));

            if (prop.Value.ValueKind == JsonValueKind.Undefined || prop.Value.ValueKind == JsonValueKind.Null) return 0;

            return (prop.Value.ValueKind == JsonValueKind.Number) ? prop.Value.GetInt32() : 0;
        }
    }
}