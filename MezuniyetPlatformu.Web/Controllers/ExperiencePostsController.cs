using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace MezuniyetPlatformu.Web.Controllers
{
    public class ExperiencePostsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public ExperiencePostsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            if (User.Identity.IsAuthenticated)
            {
                var token = User.FindFirstValue("jwt");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var posts = new List<ExperiencePostViewModel>();
            try
            {
                posts = await client.GetFromJsonAsync<List<ExperiencePostViewModel>>("ExperiencePosts");
            }
            catch {  }

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            if (User.Identity.IsAuthenticated)
            {
                var token = User.FindFirstValue("jwt");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            ExperiencePostViewModel post = null;
            try
            {
                post = await client.GetFromJsonAsync<ExperiencePostViewModel>($"ExperiencePosts/{id}");
            }
            catch
            {
                return RedirectToAction("Index");
            }

            if (post == null) return RedirectToAction("Index");

            return View(post);
        }

        [HttpGet]
        [Authorize(Roles = "Ogrenci, Mezun, Isveren")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ogrenci, Mezun, Isveren")]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? uploadedImageUrl = null;
            if (model.ImageUpload != null)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "posts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }
                uploadedImageUrl = "/uploads/posts/" + uniqueFileName;
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiDto = new { Title = model.Title, Content = model.Content, ImageUrl = uploadedImageUrl };
            var response = await client.PostAsJsonAsync("ExperiencePosts", apiDto);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            ModelState.AddModelError("", "Hata oluştu.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Like(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await client.PostAsync($"ExperiencePosts/{id}/like", null);

            return Redirect(Request.Headers["Referer"].ToString());
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return RedirectToAction("Detail", new { id = postId });
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var commentData = new { CommentText = commentText };

            var response = await client.PostAsJsonAsync($"ExperiencePosts/{postId}/comments", commentData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
            }

            return RedirectToAction("Detail", new { id = postId });
        }
    }
}