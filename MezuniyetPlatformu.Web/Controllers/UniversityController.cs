using MezuniyetPlatformu.Entities;
using MezuniyetPlatformu.Web.Models; 
using MezuniyetPlatformu.Web.ViewModels; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    //[Authorize(Roles = "UniversityRep")]
    public class UniversityController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UniversityController> _logger;

        public UniversityController(IHttpClientFactory httpClientFactory, ILogger<UniversityController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var apiBaseUrl = "https://localhost:7180";

            try
            {
                var client = _httpClientFactory.CreateClient();

                var token = User.FindFirst("jwt")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    return Content("HATA: Token bulunamadı! Lütfen çıkış yapıp tekrar giriş yapın.");
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{apiBaseUrl}/api/UniversityStats/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var viewModel = JsonSerializer.Deserialize<UniversityDashboardViewModel>(jsonData, options);

                    return View(viewModel);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Content($"API BAĞLANTI HATASI!\nDurum Kodu: {response.StatusCode}\nAPI Cevabı: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                return Content($"KRİTİK HATA!\nMesaj: {ex.Message}\nDetay: {ex.InnerException?.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> SendAnnouncement(string title, string message)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doldurun.";
                return RedirectToAction("Dashboard");
            }

            var apiBaseUrl = "https://localhost:7180";

            try
            {
                var client = _httpClientFactory.CreateClient();

                var token = User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var payload = new { Title = title, Message = message };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{apiBaseUrl}/api/Announcements/create", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Duyuru yayınlandı!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Duyuru gönderilirken bir hata oluştu.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Sunucu bağlantı hatası!";
            }

            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> MyStudents()
        {
            var apiBaseUrl = "https://localhost:7180";
            var studentList = new List<StudentViewModel>(); 

            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = User.FindFirst("jwt")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync($"{apiBaseUrl}/api/UniversityStats/students");

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    studentList = JsonSerializer.Deserialize<List<StudentViewModel>>(jsonData, options);
                }
            }
            catch (Exception)
            {
            }

            return View(studentList);
        }
        [HttpGet]
        public async Task<IActionResult> StudentDetails(int id)
        {
            var apiBaseUrl = "https://localhost:7180";

            StudentViewModel student = null;

            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"{apiBaseUrl}/api/UniversityStats/student-detail/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                student = JsonSerializer.Deserialize<StudentViewModel>(jsonData, options);
            }
            else
            {
                return RedirectToAction("MyStudents");
            }

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveGraduation(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"UniversityStats/ApproveGraduation/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Öğrenci başarıyla mezun edildi!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Hata: {errorContent}";
            }

            return RedirectToAction("StudentDetails", new { id = id });
        }

        public async Task<IActionResult> MyAnnouncements()
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var announcements = new List<Announcement>();
            try
            {
                announcements = await client.GetFromJsonAsync<List<Announcement>>("Announcements/my-announcements");
            }
            catch { }

            return View(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"Announcements/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Duyuru başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Silme işlemi başarısız.";
            }

            return RedirectToAction("MyAnnouncements");
        }

        [HttpGet]
        public async Task<IActionResult> EditAnnouncement(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var announcement = await client.GetFromJsonAsync<Announcement>($"Announcements/{id}");

            if (announcement == null) return NotFound();

            return View(announcement);
        }

        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(int id, Announcement model)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateData = new
            {
                Title = model.Title,
                Message = model.Message
            };

            var response = await client.PutAsJsonAsync($"Announcements/{id}", updateData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Duyuru başarıyla güncellendi.";
                return RedirectToAction("MyAnnouncements");
            }
            else
            {
                TempData["ErrorMessage"] = "Güncelleme başarısız.";
                return View(model);
            }
        }
    }
}