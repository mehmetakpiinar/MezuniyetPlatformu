using MezuniyetPlatformu.Web.ViewModels;
using MezuniyetPlatformu.Web.Models; 
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace MezuniyetPlatformu.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("Landing");
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardModel = new DashboardViewModel();

            try
            {
                var stats = await client.GetFromJsonAsync<DashboardViewModel>("Dashboard/summary");
                if (stats != null) dashboardModel = stats;
            }
            catch
            {
            }

            int profileScore = 0;
            try
            {
                var response = await client.GetAsync("Profiller/me");

                if (response.IsSuccessStatusCode)
                {
                    var myProfile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

                    if (myProfile != null)
                    {
                        profileScore += 20;

                        if (!string.IsNullOrEmpty(myProfile.PhoneNumber)) profileScore += 15;

                        if (!string.IsNullOrEmpty(myProfile.About)) profileScore += 20;

                        if (!string.IsNullOrEmpty(myProfile.ProfilePhotoURL)) profileScore += 15;

                        if (!string.IsNullOrEmpty(myProfile.LinkedInURL) || !string.IsNullOrEmpty(myProfile.GitHubURL)) profileScore += 15;

                        if (!string.IsNullOrEmpty(myProfile.Skills)) profileScore += 15;
                    }
                }
            }
            catch
            {
                profileScore = 0; 
            }

            dashboardModel.ProfileCompletionRate = profileScore > 100 ? 100 : profileScore;

            dashboardModel.ChartData = new List<int> { 5, 12, 8, 15, 10, dashboardModel.MyApplications };

            dashboardModel.Role = User.IsInRole("Isveren") ? "Isveren" : "Mezun";

            try
            {
                var allPosts = await client.GetFromJsonAsync<List<JobPostsViewModel>>("JobPosts");
                if (allPosts != null)
                {
                    ViewBag.RecentPosts = allPosts.Take(4).ToList();
                }
            }
            catch
            {
                ViewBag.RecentPosts = new List<JobPostsViewModel>();
            }

            return View(dashboardModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Announcements()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Auth");

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var list = await client.GetFromJsonAsync<List<MezuniyetPlatformu.Entities.Announcement>>("Announcements/student-list");

            return View(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       
    }
}