
using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; 

namespace MezuniyetPlatformu.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public EventsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var events = new List<EventViewModel>();

            try
            {
                events = await client.GetFromJsonAsync<List<EventViewModel>>("Events");
                if (events != null)
                {
                    events = events.OrderBy(e => e.EventDate).ToList();
                }
            }
            catch
            {
            }

            return View(events);
        }

        [Authorize(Roles = "UniversityRep")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "UniversityRep")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            model.UniversityId = 1;
            ModelState.Remove("UniversityId");

            if (!ModelState.IsValid) return View(model);

            string? uploadedImageUrl = null;

            if (model.ImageUpload != null && model.ImageUpload.Length > 0)
            {
                var fileExtension = Path.GetExtension(model.ImageUpload.FileName);

                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = ".jpg";
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "events");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }

                uploadedImageUrl = "/uploads/events/" + uniqueFileName;
            }

            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiData = new
            {
                Title = model.Title,
                Description = model.Description,
                EventDate = model.EventDate,
                Location = model.Location,
                UniversityId = model.UniversityId,
                ImageUrl = uploadedImageUrl
            };

            var response = await client.PostAsJsonAsync("Events", apiData);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Hata: {errorContent}";
            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            EventViewModel ev = null;

            try
            {
                ev = await client.GetFromJsonAsync<EventViewModel>($"Events/{id}");
            }
            catch
            {
                return RedirectToAction("Index");
            }

            if (ev == null) return RedirectToAction("Index");

            return View(ev);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"Events/{id}/join", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "İşlem başarılı!";
            }
            else
            {
                TempData["ErrorMessage"] = "Bir hata oluştu.";
            }

            return RedirectToAction("Detail", new { id = id });
        }
    }
}
