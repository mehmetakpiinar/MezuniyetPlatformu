// MezuniyetPlatformu.Web/Controllers/AuthController.cs

using MezuniyetPlatformu.Web.ViewModels; // ViewModellarımızı kullanabilmek için
using Microsoft.AspNetCore.Mvc;
using System.Text; // StringContent için
using System.Text.Json; // JsonSerializer için
using System.Threading.Tasks; // async Task için
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // JWT Token'ı okumak için

namespace MezuniyetPlatformu.Web.Controllers
{
    public class AuthController : Controller
    {
        // API ile konuşmamızı sağlayacak olan HttpClient için fabrika
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthController> _logger; // Hata ayıklama için

        // Constructor (Yapıcı Metot) ile servisleri enjekte ediyoruz
        public AuthController(IHttpClientFactory httpClientFactory, ILogger<AuthController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Register'dan gelen başarı mesajı varsa onu View'a taşı
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        // Kayıt Ol formundan gelen verileri karşılar
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarını önlemek için
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // 1. Model Geçerli mi? (ViewModel'daki [Required], [Compare] vb. kuralları geçti mi?)
            if (!ModelState.IsValid)
            {
                // Geçmediyse, formu hata mesajlarıyla birlikte kullanıcıya geri göster
                return View(model);
            }

            // 2. API ile konuşacak olan istemciyi oluştur
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 3. API'mizin beklediği DTO formatına veriyi dönüştür
            //    (SENİN API DTO'N İNGİLİZCE OLDUĞU İÇİN BURASI DA İNGİLİZCE)
            var apiRequestData = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password, // API'deki DTO 'Password' bekliyor
                UserTypeId = model.UserTypeId
            };

            // 4. Veriyi JSON formatına çevir
            var content = new StringContent(JsonSerializer.Serialize(apiRequestData), Encoding.UTF8, "application/json");

            // 5. API'mizin "auth/register" endpoint'ine POST isteği gönder
            _logger.LogInformation("API'ye kayıt isteği gönderiliyor: {Email}", model.Email);
            var response = await client.PostAsync("auth/register", content);

            // 6. API'den gelen cevabı kontrol et
            if (response.IsSuccessStatusCode)
            {
                // Kayıt başarılıysa (200 OK, 201 Created vb.)
                _logger.LogInformation("API'den başarılı kayıt cevabı alındı: {Email}", model.Email);

                // Kullanıcıya bir başarı mesajı göster ve Login sayfasına yönlendir
                TempData["SuccessMessage"] = "Kayıt işlemi başarıyla tamamlandı! Lütfen giriş yapın.";
                return RedirectToAction("Login");
            }
            else
            {
                // API hata döndürdüyse (örn: 400 Bad Request - "Email zaten kullanılıyor")
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API'den kayıt hatası alındı: {StatusCode} - {Error}", response.StatusCode, errorContent);

                // Bu hatayı formun en üstünde göstermek için ModelState'e ekle
                ModelState.AddModelError(string.Empty, "Kayıt başarısız: " + errorContent);

                // Formu kullanıcıya hatalarla geri göster
                return View(model);
            }
        }

        // ... [HttpPost] Register metodunun sonu ...

        // ---- YENİ EKLENEN LOGIN METODU ----

        // POST: /Auth/Login
        // Giriş Yap formundan gelen verileri karşılar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // 1. Model Geçerli mi?
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. API ile konuşacak istemciyi oluştur
            var client = _httpClientFactory.CreateClient("ApiClient");

            // 3. API'mizin beklediği DTO formatına veriyi dönüştür
            //    (Senin API LoginDto'n Email ve Sifre bekliyordu,
            //     ancak API DTO'n 'Password' bekliyorsa 'Sifre = model.Sifre' yerine 'Password = model.Sifre' yaz)
            var apiRequestData = new
            {
                Email = model.Email,
                Sifre = model.Sifre // veya 'Password = model.Sifre'
            };

            // 4. Veriyi JSON'a çevir ve API'ye gönder
            var content = new StringContent(JsonSerializer.Serialize(apiRequestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("auth/login", content);

            // 5. API'den gelen cevabı kontrol et
            if (response.IsSuccessStatusCode)
            {
                // BAŞARILI GİRİŞ (API 200 OK ve Token Döndürdü)

                // 6. API'den dönen JSON'ı oku (örn: { "token": "eyJ..." })
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // JSON'ı geçici bir nesneye ayıkla
                var tokenObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                var token = tokenObject.GetProperty("token").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError(string.Empty, "API'den geçerli bir token alınamadı.");
                    return View(model);
                }

                _logger.LogInformation("Kullanıcı {Email} başarıyla giriş yaptı, token alındı.", model.Email);

                // 7. SİTEDE OTURUM AÇMA (COOKIE OLUŞTURMA)
                await SignInUser(token);

                // 8. Kullanıcıyı Ana Sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // BAŞARISIZ GİRİŞ (API 401 Unauthorized - "Geçersiz şifre" vb.)
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API'den giriş hatası alındı: {Error}", errorContent);
                ModelState.AddModelError(string.Empty, "Giriş başarısız: " + errorContent);
                return View(model);
            }
        }

        // ---- YENİ EKLENEN LOGOUT METODU ----

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cookie Authentication sistemindeki oturum çerezini siler
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Kullanıcı oturumu kapattı.");

            // Kullanıcıyı Ana Sayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }

        // ---- YENİ EKLENEN YARDIMCI METOT ----

        // Bu metot, API'den gelen JWT'yi okur ve bir Cookie oluşturur
        // Bu metot, API'den gelen JWT'yi okur ve bir Cookie oluşturur
        private async Task SignInUser(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Token'ın içindeki "Claim"leri (bilgileri) oku
            var claims = jwtToken.Claims.ToList();

            // Token'ı da bir claim olarak ekle. Bu, gelecekte API'ye istek atarken
            // bu token'ı kullanabilmemizi sağlar.
            claims.Add(new Claim("jwt", token));

            // ---- DÜZELTİLMİŞ KISIM ----
            // Cookie Authentication için bir kimlik oluştur
            // ve ona 'Name' (İsim) ve 'Role' (Rol) için HANGİ claim'e bakacağını SÖYLE
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Email,  // User.Identity.Name olarak 'Email' claim'ini kullan
                "role"   // User.IsInRole() için 'Role' claim'ini kullan
            );
            // ---- DÜZELTMENİN SONU ----

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Tarayıcıyı kapatsa bile hatırla
                ExpiresUtc = jwtToken.ValidTo // Cookie'nin süresi, token'ın süresiyle aynı olsun
            };

            // Kullanıcının oturumunu (cookie) oluştur
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}