using MezuniyetPlatformu.Web.ViewModels; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http; 
using System.Net.Http.Headers; 
using System.Net.Http.Json; 
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> Index(string? s, string? sehir, string? tur)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            ViewBag.SearchTerm = s;
            ViewBag.Location = sehir;
            ViewBag.JobType = tur;

            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(s)) queryParams.Add($"s={s}");
            if (!string.IsNullOrEmpty(sehir)) queryParams.Add($"sehir={sehir}");
            if (!string.IsNullOrEmpty(tur)) queryParams.Add($"tur={tur}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            List<JobPostsViewModel> jobPostings = null;

            try
            {
                jobPostings = await client.GetFromJsonAsync<List<JobPostsViewModel>>("JobPosts" + queryString);
            }
            catch (HttpRequestException ex)
            {
                jobPostings = new List<JobPostsViewModel>();
            }

            return View(jobPostings);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            _logger.LogInformation("API'ye GET /api/JobPosts/{id} isteği gönderiliyor...", id);
            JobPostsViewModel jobPost = null;

            try
            {
                jobPost = await client.GetFromJsonAsync<JobPostsViewModel>($"JobPosts/{id}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("API'den ilan detayı çekilirken hata oluştu: {Error}", ex.Message);

                TempData["ErrorMessage"] = "İlan bulunamadı veya bir hata oluştu.";
                return RedirectToAction("Index"); 
            }

            if (jobPost == null)
            {
                TempData["ErrorMessage"] = "İlan bulunamadı.";
                return RedirectToAction("Index");
            }

            return View(jobPost);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> Apply(int id)
        {
            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Kullanıcı {UserId} için JWT token cookie'de bulunamadı.", User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["ErrorMessage"] = "Oturumunuzda bir sorun oluştu. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Kullanıcı {UserId}, ilan {IlanId} için API'ye başvuru isteği gönderiyor.", User.FindFirstValue(ClaimTypes.NameIdentifier), id);

            var response = await client.PostAsync($"JobPosts/{id}/apply", null);

            if (response.IsSuccessStatusCode) // 201 Created
            {
                _logger.LogInformation("API'den başarılı başvuru cevabı alındı.");
                TempData["SuccessMessage"] = "İlana başarıyla başvurdunuz!";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) // 400
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API başvuru hatası (BadRequest): {Error}", errorContent);
                TempData["ErrorMessage"] = "Başvuru başarısız: " + errorContent;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("API başvuru hatası ({StatusCode}): {Error}", response.StatusCode, errorContent);
                TempData["ErrorMessage"] = "Başvuru sırasında bir hata oluştu. Lütfen tekrar deneyin.";
            }

            return RedirectToAction("Detail", new { id = id });
        }


        [HttpGet]
        [Authorize(Roles = "Isveren")] 
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> Create(CreateJobPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("JobPosts", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "İlanınız başarıyla yayınlandı!";
                return RedirectToAction("MyPosts");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("İlan oluşturma hatası: {Error}", errorContent);
                ModelState.AddModelError(string.Empty, "İlan oluşturulamadı: " + errorContent);
                return View(model);
            }
        }


        [HttpGet]
        [Authorize(Roles = "Isveren")] 
        public async Task<IActionResult> Applications(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<JobApplicationViewModel> applications = new List<JobApplicationViewModel>();

            try
            {
                applications = await client.GetFromJsonAsync<List<JobApplicationViewModel>>($"JobPosts/{id}/applications");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Başvurular çekilirken hata: {Error}", ex.Message);
                TempData["ErrorMessage"] = "Başvurular yüklenemedi veya bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            ViewBag.JobPostId = id;

            return View(applications);
        }
        [Authorize(Roles = "Ogrenci, Mezun")]
        public async Task<IActionResult> MyApplications()
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<JobApplicationViewModel> myApps = new List<JobApplicationViewModel>();

            try
            {
                myApps = await client.GetFromJsonAsync<List<JobApplicationViewModel>>("JobPosts/my-applications");
            }
            catch (Exception ex)
            {
                _logger.LogError("Başvurularım yüklenirken hata: {Error}", ex.Message);
            }

            return View(myApps);
        }
        [HttpGet]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> MyPosts()
        {
            var apiBaseUrl = "https://localhost:7180";
            var myPosts = new List<JobPostsViewModel>();

            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"{apiBaseUrl}/api/JobPosts/my-posts");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                myPosts = JsonSerializer.Deserialize<List<JobPostsViewModel>>(jsonData, options);
            }

            return View(myPosts);
        }

        [HttpGet]
        [Authorize(Roles = "Isveren")]
        public async Task<IActionResult> ManageApplications(int id)
        {
            var apiBaseUrl = "https://localhost:7180";
            var applications = new List<JobApplicationViewModel>();

            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync($"{apiBaseUrl}/api/JobPosts/{id}/applications");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                applications = JsonSerializer.Deserialize<List<JobApplicationViewModel>>(jsonData, options);
            }

            ViewBag.JobPostId = id;
            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"JobPosts/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "İlan başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "İlan silinirken bir hata oluştu veya yetkiniz yok.";
            }

            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("MyPosts");
        }
    }
}