using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar profilini görebilir
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IHttpClientFactory httpClientFactory, ILogger<ProfileController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /Profile/Index
        // Kullanıcının kendi profilini görüntüler
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ProfileViewModel profile = null;

            try
            {
                // API'deki "ProfillerController"a istek atıyoruz
                // Adres: .../api/Profiller/me
                profile = await client.GetFromJsonAsync<ProfileViewModel>("Profiller/me");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Profil çekilemedi: {Error}", ex.Message);
                // Eğer profil yoksa (404) veya hata varsa boş bir modelle dönebiliriz
                // Veya API 404 döndüyse bu kullanıcının henüz profili oluşmamış demektir.
            }

            return View(profile);
        }

        // GET: /Profile/Edit
        // Düzenleme sayfasını gösterir
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // Index ile aynı mantık, veriyi çekip forma dolduracağız
            return await Index();
        }

        // POST: /Profile/Edit
        // Güncellenmiş verileri API'ye gönderir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // API'ye göndereceğimiz DTO (UpdateProfileDto ile eşleşmeli)
            var updateData = new
            {
                ProfilePhotoURL = model.ProfilePhotoURL,
                About = model.About,
                GraduationYear = model.GraduationYear,
                StudyProgram = model.StudyProgram,
                LinkedInURL = model.LinkedInURL,
                GitHubURL = model.GitHubURL,
                PhoneNumber = model.PhoneNumber
            };

            var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");

            // API: PUT /api/Profiller/me
            var response = await client.PutAsync("Profiller/me", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Güncelleme başarısız oldu.";
                return View(model);
            }
        }
    }
}