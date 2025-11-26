// MezuniyetPlatformu.Web/Controllers/JobPostingsController.cs

using MezuniyetPlatformu.Web.ViewModels; // ViewModel'lar için
using Microsoft.AspNetCore.Authorization; // [Authorize] için
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // List<> için
using System.Net.Http; // HttpClient için
using System.Net.Http.Headers; // "Bearer" token'ı eklemek için
using System.Net.Http.Json; // GetFromJsonAsync için
using System.Security.Claims; // Token'dan ID okumak için
using System.Text;
using System.Text.Json;
using System.Threading.Tasks; // async Task için

namespace MezuniyetPlatformu.Web.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JobPostsController> _logger;

        public JobPostsController(IHttpClientFactory httpClientFactory, ILogger<JobPostsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: /JobPostings/Index
        // VEYA sadece /JobPostings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. API ile konuşacak istemciyi oluştur
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 2. API'nin herkese açık olan 'JobPostings' endpoint'ine GET isteği at
            _logger.LogInformation("API'ye GET /api/JobPostings isteği gönderiliyor...");

            // ---- DÜZELTİLMİŞ SATIR 1 ----
            List<JobPostsViewModel> jobPostings = null; // 'JobPostingViewModel' DEĞİL, 'JobPostsViewModel' (çoğul)

            try
            {
                // ---- DÜZELTİLMİŞ SATIR 2 ----
                // API'den gelen JSON dizisini doğrudan bir List<JobPostsViewModel>'a çevir
                jobPostings = await client.GetFromJsonAsync<List<JobPostsViewModel>>("JobPosts"); // 'JobPostingViewModel' DEĞİL, 'JobPostsViewModel' (çoğul)

                _logger.LogInformation("{Count} adet iş ilanı API'den başarıyla çekildi.", jobPostings.Count);
            }
            catch (HttpRequestException ex)
            {
                // API'ye ulaşılamazsa veya API hata dönerse (örn: 500)
                _logger.LogError("API'ye bağlanırken hata oluştu: {Error}", ex.ToString());

                // ESKİ KOD:
                // jobPostings = new List<JobPostsViewModel>();
                // ViewBag.ErrorMessage = "İş ilanları yüklenirken bir sorun oluştu.";

                // YENİ KOD (Hatanın iç detayını göstermek için):
                jobPostings = new List<JobPostsViewModel>();
                ViewBag.ErrorMessage = $"API Bağlantı Hatası: {ex.Message}";

                if (ex.InnerException != null)
                {
                    ViewBag.ErrorMessage += $" | Detay: {ex.InnerException.Message}";
                }
            }

            // 3. Modeli (ilan listesini) View'a (sayfaya) gönder
            return View(jobPostings);
        }

        // BİR SONRAKİ ADIM: /JobPostings/Detail/{id} metodu buraya eklenecek.
        // ... (Controller'ın içindesin) ...
        // ... (Index [HttpGet] metodu burada) ...

        // ---- YENİ EKLENEN METOT ----

        // GET: /JobPostings/Detail/5 (Örnek: 5 ID'li ilanın detayını getirir)
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            // 1. API ile konuşacak istemciyi oluştur
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 2. API'nin 'JobPosts/{id}' endpoint'ine GET isteği at
            _logger.LogInformation("API'ye GET /api/JobPosts/{id} isteği gönderiliyor...", id);
            JobPostsViewModel jobPost = null;

            try
            {
                // API'den gelen JSON nesnesini doğrudan JobPostsViewModel'a çevir
                // Adres: .../api/JobPosts/5
                jobPost = await client.GetFromJsonAsync<JobPostsViewModel>($"JobPosts/{id}");
            }
            catch (HttpRequestException ex)
            {
                // API'ye ulaşılamazsa veya API 404 (bulunamadı) dönerse
                _logger.LogError("API'den ilan detayı çekilirken hata oluştu: {Error}", ex.Message);

                // Kullanıcıyı 404 sayfasına (veya ana sayfaya) yönlendir
                TempData["ErrorMessage"] = "İlan bulunamadı veya bir hata oluştu.";
                return RedirectToAction("Index"); // Hata olursa İlan Listesine geri dön
            }

            if (jobPost == null)
            {
                TempData["ErrorMessage"] = "İlan bulunamadı.";
                return RedirectToAction("Index");
            }

            // 3. Modeli (tek bir ilan) View'a (sayfaya) gönder
            // Bu kod, Views/JobPosts/Detail.cshtml dosyasını arayacak
            return View(jobPost);
        }
        // ... (Controller'ın içindesin) ...
        // ... (Detail [HttpGet] metodu burada) ...

        // ---- YENİ EKLENEN METOT ----

        // POST: /JobPostings/Apply/5
        // [ValidateAntiForgeryToken] -> Bizim formumuz <form ... method="post"> olduğu için
        // .NET bunu otomatik olarak ekler ve CSRF saldırılarına karşı korur.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Sadece bu rollere sahip olanlar bu metodu tetikleyebilir
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> Apply(int id)
        {
            // 1. GEREKLİ TOKEN'I AL
            // AuthController'da Login olurken Cookie'ye "jwt" adıyla kaydettiğimiz token'ı alıyoruz
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Kullanıcı {UserId} için JWT token cookie'de bulunamadı.", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "Oturumunuzda bir sorun oluştu. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            // 2. API İSTEMCİSİNİ OLUŞTUR VE TOKEN'I EKLE
            var client = _httpClientFactory.CreateClient("ApiClient");
            // API isteğinin header'ına "Authorization: Bearer <token>" bilgisini ekliyoruz
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. API'NİN "APPLY" ENDPOINT'İNE İSTEK AT
            // Bu, API'deki POST /api/JobPosts/{id}/apply adresini çağırır
            // Başvuru işlemi bir POST'tur ancak JSON body'si göndermemiz gerekmez (content: null)
            _logger.LogInformation("Kullanıcı {UserId}, ilan {IlanId} için API'ye başvuru isteği gönderiyor.", User.FindFirstValue(ClaimTypes.NameIdentifier), id);

            // ÖNEMLİ: API adresinin "JobPosts" mu "JobPostings" mi olduğunu kontrol et!
            // Bir önceki hatadan yola çıkarak "JobPosts" olduğunu varsayıyorum.
            var response = await client.PostAsync($"JobPosts/{id}/apply", null);

            // 4. API'DEN GELEN CEVABI KONTROL ET
            if (response.IsSuccessStatusCode) // 201 Created
            {
                _logger.LogInformation("API'den başarılı başvuru cevabı alındı.");
                // Başarılı olursa, kullanıcıyı bir başarı mesajıyla Detay sayfasına geri yönlendir
                TempData["SuccessMessage"] = "İlana başarıyla başvurdunuz!";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) // 400
            {
                // API "Bu ilana zaten başvurmuşsunuz" hatası döndürdü
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API başvuru hatası (BadRequest): {Error}", errorContent);
                TempData["ErrorMessage"] = "Başvuru başarısız: " + errorContent;
            }
            else
            {
                // Diğer hatalar (401, 403, 404, 500)
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API başvuru hatası ({StatusCode}): {Error}", response.StatusCode, errorContent);
                TempData["ErrorMessage"] = "Başvuru sırasında bir hata oluştu. Lütfen tekrar deneyin.";
            }

            // 5. Her durumda kullanıcıyı ilanın detay sayfasına geri yönlendir
            return RedirectToAction("Detail", new { id = id });
        }


        [HttpGet]
        [Authorize(Roles = "Isveren")] // Sadece İşverenler girebilir (Veritabanındaki isme dikkat: "Employer" olabilir!)
                                       // EĞER VERİTABANINDA "Employer" İSE: [Authorize(Roles = "Employer")] YAPMALISIN
        public IActionResult Create()
        {
            return View();
        }

        // POST: /JobPosts/Create
        // Form verilerini API'ye gönderir
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Isveren")] // Veya "Employer"
        public async Task<IActionResult> Create(CreateJobPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Veriyi JSON'a çevir
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            // API'ye POST isteği at (API Adresi: POST /api/JobPosts)
            var response = await client.PostAsync("JobPosts", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "İlanınız başarıyla yayınlandı!";
                // İlanları listeleme sayfasına dön
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("İlan oluşturma hatası: {Error}", errorContent);
                // Eğer profil yoksa API "şirket profiline sahip olmalısınız" diyebilir.
                ModelState.AddModelError(string.Empty, "İlan oluşturulamadı: " + errorContent);
                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Isveren")] // Sadece işverenler
        public async Task<IActionResult> Applications(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<JobApplicationViewModel> applications = new List<JobApplicationViewModel>();

            try
            {
                // API: GET /api/JobPosts/{id}/applications
                applications = await client.GetFromJsonAsync<List<JobApplicationViewModel>>($"JobPosts/{id}/applications");
            }
            catch (HttpRequestException ex)
            {
                // Eğer 403 alırsak (Başkasına ait ilana bakmaya çalışıyorsa)
                // API bize hata kodu dönecektir.
                _logger.LogError("Başvurular çekilirken hata: {Error}", ex.Message);
                TempData["ErrorMessage"] = "Başvurular yüklenemedi veya bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            // Hangi ilanın başvurularına baktığımızı bilmek için ID'yi View'a taşıyalım
            ViewBag.JobPostId = id;

            return View(applications);
        }
    }
}