using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace MezuniyetPlatformu.Web.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = GetClient();
            var stats = await client.GetFromJsonAsync<AdminStatsViewModel>("Admin/stats");
            return View(stats);
        }

        public async Task<IActionResult> Users()
        {
            var client = GetClient();
            var users = await client.GetFromJsonAsync<List<AdminUserViewModel>>("Admin/users");
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"Admin/users/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
            else
                TempData["ErrorMessage"] = "Kullanıcı silinemedi.";

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJobPost(int id)
        {
            var client = GetClient();
            await client.DeleteAsync($"Admin/jobposts/{id}");
            TempData["SuccessMessage"] = "İlan silindi.";
            return RedirectToAction("JobPosts", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExperiencePost(int id)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"Admin/experienceposts/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Paylaşım başarıyla silindi.";
            }
            else
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Silme Başarısız: {errorDetail}";
            }
            return RedirectToAction("ExperiencePosts");
        }

        private HttpClient GetClient()
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<IActionResult> Universities()
        {
            var client = GetClient();
            var unis = await client.GetFromJsonAsync<List<UniversityViewModel>>("Admin/universities");
            return View(unis);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUniversity(string Name, string LogoUrl)
        {
            var client = GetClient();

            var newUni = new { Name = Name, LogoUrl = LogoUrl };
            var response = await client.PostAsJsonAsync("Admin/universities", newUni);

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Üniversite başarıyla eklendi.";
            else
                TempData["ErrorMessage"] = "Ekleme başarısız.";

            return RedirectToAction("Universities");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"Admin/universities/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Üniversite silindi.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Silinemedi: {errorMsg}";
            }

            return RedirectToAction("Universities");
        }


        public async Task<IActionResult> JobPosts()
        {
            var client = GetClient();
            var posts = await client.GetFromJsonAsync<List<AdminJobPostViewModel>>("Admin/jobposts");
            return View(posts);
        }

        public async Task<IActionResult> ExperiencePosts()
        {
            var client = GetClient();
            var posts = await client.GetFromJsonAsync<List<AdminExperiencePostViewModel>>("Admin/experienceposts");
            return View(posts);
        }
    }
}