// MezuniyetPlatformu.Web/Controllers/ExperiencePostsController.cs

using MezuniyetPlatformu.Web.ViewModels; // ViewModel'lar için
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // List<> için
using System.Net.Http; // HttpClient için
using System.Net.Http.Json; // GetFromJsonAsync için
using System.Threading.Tasks; // async Task için
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace MezuniyetPlatformu.Web.Controllers
{
    public class ExperiencePostsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExperiencePostsController> _logger;

        public ExperiencePostsController(IHttpClientFactory httpClientFactory, ILogger<ExperiencePostsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /ExperiencePosts
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            _logger.LogInformation("API'ye GET /api/ExperiencePosts isteği gönderiliyor...");

            List<ExperiencePostViewModel> posts = null;

            try
            {
                // API'den gelen JSON dizisini List<ExperiencePostViewModel>'a çevir
                posts = await client.GetFromJsonAsync<List<ExperiencePostViewModel>>("ExperiencePosts");
                _logger.LogInformation("{Count} adet deneyim paylaşımı API'den çekildi.", posts.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("API'ye bağlanırken hata oluştu: {Error}", ex.Message);
                posts = new List<ExperiencePostViewModel>();
                ViewBag.ErrorMessage = "Deneyim paylaşımları yüklenirken bir sorun oluştu.";

                if (ex.InnerException != null)
                {
                    ViewBag.ErrorMessage += $" | Detay: {ex.InnerException.Message}";
                }
            }

            // Modeli (paylaşım listesini) View'a (sayfaya) gönder
            return View(posts);
        }

        // ... (Index metodu yukarıda) ...

        // GET: /ExperiencePosts/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            _logger.LogInformation("API'ye GET /api/ExperiencePosts/{id} isteği gönderiliyor...", id);

            ExperiencePostViewModel post = null;

            try
            {
                // API'den tek bir paylaşımı çek
                // URL: .../api/ExperiencePosts/5
                post = await client.GetFromJsonAsync<ExperiencePostViewModel>($"ExperiencePosts/{id}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("API'den paylaşım detayı çekilirken hata oluştu: {Error}", ex.Message);
                TempData["ErrorMessage"] = "Paylaşım bulunamadı veya bir hata oluştu.";
                return RedirectToAction("Index");
            }

            if (post == null)
            {
                TempData["ErrorMessage"] = "Paylaşım bulunamadı.";
                return RedirectToAction("Index");
            }

            return View(post);
        }
        // ... Detail metodu bitti ...

        // ---- YENİ METOTLAR ----

        // GET: /ExperiencePosts/Create
        // Paylaşım formunu gösterir
        [HttpGet]
        [Authorize(Roles = "Ogrenci, Mezun")] // Sadece yetkili roller girebilir
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ExperiencePosts/Create
        // Formdan gelen veriyi API'ye gönderir
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Token'ı Cookie'den al
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            // 2. API İstemcisini Hazırla ve Token'ı Ekle
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. Veriyi JSON'a çevir
            // (API'deki CreatePostDto ile isimlerin eşleştiğinden emin ol: Title, Content)
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            // 4. API'ye POST isteği at
            var response = await client.PostAsync("ExperiencePosts", content);

            if (response.IsSuccessStatusCode) // 201 Created
            {
                TempData["SuccessMessage"] = "Deneyiminiz başarıyla paylaşıldı!";
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Paylaşım oluşturulurken hata: {Error}", errorContent);
                ModelState.AddModelError(string.Empty, "Paylaşım yapılamadı: " + errorContent);
                return View(model);
            }
        }
    }
}