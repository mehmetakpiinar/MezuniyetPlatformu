using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace MezuniyetPlatformu.Business
{
    public class AiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AiService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["Gemini:ApiKey"];
            _httpClient = httpClient;
        }

        public async Task<string> GenerateAboutMeAsync(string bolum, string uni, string yetenekler)
        {
            try
            {
                var prompt = $"Üniversite mezuniyet platformu profilim için profesyonel bir 'Hakkımda' yazısı yazar mısın? Bilgiler: {uni} mezunuyum, {bolum} bölümü okudum. Yeteneklerim: {yetenekler}. Sadece metni döndür.";

                var requestBody = new
                {
                    contents = new[]
    {
        new
        {
            role = "user",
            parts = new[]
            {
                new { text = prompt }
            }
        }
    }
                };


                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // EN GÜNCEL VE KARARLI URL: v1beta ve gemini-1.5-flash-latest kombinasyonu
                var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={_apiKey}";


                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Hatanın nedenini tam olarak burada göreceğiz
                    return $"API_ERROR: {response.StatusCode} - {responseBody}";
                }

                using var doc = JsonDocument.Parse(responseBody);

                // JSON okuma kısmını en esnek hale getirdik
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates[0].TryGetProperty("content", out var content))
                {
                    return content.GetProperty("parts")[0].GetProperty("text").GetString();
                }

                return "AI yanıt üretti ancak içerik okunamadı.";
            }
            catch (Exception ex)
            {
                return $"SYSTEM_ERROR: {ex.Message}";
            }
        }
    }
}