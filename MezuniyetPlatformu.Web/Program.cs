using MezuniyetPlatformu.Business;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles; 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ---- API BAĞLANTISI (HttpClientFactory) ----
builder.Services.AddHttpClient("ApiClient", client =>
{
    
    client.BaseAddress = new Uri("https://localhost:7180/api/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
// SSL HATALARINI YOKSAYMAK İÇİN
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        (message, cert, chain, errors) => { return true; };
    return handler;
});

// ---- WEB SİTESİ GİRİŞ (Cookie Authentication) ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.SlidingExpiration = true;
    });

// AiService için HttpClient yapılandırması
builder.Services.AddHttpClient<AiService>(client =>
{
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
    return handler;
});

var app = builder.Build();


app.UseHttpsRedirection();


var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
// provider.Mappings[".jfif"] = "image/jpeg";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();