using MezuniyetPlatformu.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MezuniyetPlatformu.Web.Controllers
{
    [Authorize] 
    public class MessagesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(IHttpClientFactory httpClientFactory, ILogger<MessagesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.FindFirstValue("jwt");
            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<ConversationViewModel> conversations = new List<ConversationViewModel>();

            try
            {
                conversations = await client.GetFromJsonAsync<List<ConversationViewModel>>("Messages/contacts");
            }
            catch
            {
            }

            return View(conversations);
        }
        [HttpGet]
        public async Task<IActionResult> Chat(int userId)
        {
            var myUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(myUserIdString))
            {
                myUserIdString = User.FindFirstValue("nameid");
            }

            if (string.IsNullOrEmpty(myUserIdString))
            {
                return RedirectToAction("Login", "Auth");
            }

            var myUserId = int.Parse(myUserIdString);

            var token = User.FindFirstValue("jwt");

            var client = _httpClientFactory.CreateClient("ApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<MessageViewModel> messages = new List<MessageViewModel>();
            try
            {
                messages = await client.GetFromJsonAsync<List<MessageViewModel>>($"Messages/{userId}");
            }
            catch (HttpRequestException)
            {
                messages = new List<MessageViewModel>();
            }

            if (messages != null)
            {
                foreach (var msg in messages)
                {
                    msg.IsMyMessage = (msg.SenderUserId == myUserId);
                }
            }

            ViewBag.OtherUserId = userId;

            return View(messages);
        }

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

            var messageDto = new
            {
                RecipientUserId = recipientUserId,
                Content = content
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(messageDto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Messages", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Mesaj gönderilemedi! Hata Kodu: {response.StatusCode}. Detay: {errorContent}";
            }

            return RedirectToAction("Chat", new { userId = recipientUserId });
        }
    }
}