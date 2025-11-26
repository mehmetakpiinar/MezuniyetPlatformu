using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar mesajlaşabilir
    public class MessagesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IHttpClientFactory httpClientFactory, ILogger<MessagesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Bu sadece "Mesajlar" linkine tıklandığında açılacak boş bir sayfa olabilir
        // Veya sohbet geçmişi olan kişilerin listesi (API'de bu özellik yoksa boş geçebiliriz)
        public IActionResult Index()
        {
            // Şimdilik boş bir sayfa veya "Sohbet etmek için bir profile gidin" mesajı
            return View();
        }

        // GET: /Messages/Chat?userId=5
        // Belirli bir kullanıcı (userId) ile olan sohbeti açar
        [HttpGet]
        public async Task<IActionResult> Chat(int userId)
        {
            // ---- GÜVENLİ ID ALMA BLOĞU (DÜZELTİLDİ) ----
            // 1. Önce resmi isimle (http://.../nameidentifier) dene
            var myUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Bulamazsan kısa isimle ("nameid") dene (JWT genelde bunu kullanır)
            if (string.IsNullOrEmpty(myUserIdString))
            {
                myUserIdString = User.FindFirstValue("nameid");
            }

            // 3. Hala bulamazsan oturum bozuktur, Login'e at
            if (string.IsNullOrEmpty(myUserIdString))
            {
                return RedirectToAction("Login", "Auth");
            }

            var myUserId = int.Parse(myUserIdString);
            // ---------------------------------------------

            var token = User.FindFirstValue("jwt");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. API'den sohbet geçmişini çek
            List<MessageViewModel> messages = new List<MessageViewModel>();
            try
            {
                // Hata yakalama bloğu: Eğer sohbet yoksa veya API hata verirse boş liste dönsün
                messages = await client.GetFromJsonAsync<List<MessageViewModel>>($"Messages/{userId}");
            }
            catch (HttpRequestException)
            {
                // İlk defa konuşuluyorsa veya hata varsa boş liste dönebilir
                messages = new List<MessageViewModel>();
            }

            // 3. Mesajların hangisi "Benim" hangisi "Onun" işaretle
            if (messages != null)
            {
                foreach (var msg in messages)
                {
                    msg.IsMyMessage = (msg.SenderUserId == myUserId);
                }
            }

            // 4. Karşı tarafın ID'sini View'a taşı (Yeni mesaj atarken lazım olacak)
            ViewBag.OtherUserId = userId;

            // Sohbeti en sona kaydırmak için
            return View(messages);
        }

        // POST: /Messages/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int recipientUserId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Chat", new { userId = recipientUserId });
            }

            var token = User.FindFirstValue("jwt");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // API DTO formatı
            var messageDto = new
            {
                RecipientUserId = recipientUserId,
                Content = content
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(messageDto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Messages", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                // ---- GÜNCEL: HATA DETAYINI OKU ----
                var errorContent = await response.Content.ReadAsStringAsync();
                // Hata kodunu ve mesajını ekrana yazdır
                TempData["ErrorMessage"] = $"Mesaj gönderilemedi! Hata Kodu: {response.StatusCode}. Detay: {errorContent}";
            }

            // Mesajı gönderdikten sonra sohbet sayfasına geri dön (sayfa yenilenir ve yeni mesaj görünür)
            return RedirectToAction("Chat", new { userId = recipientUserId });
        }
    }
}