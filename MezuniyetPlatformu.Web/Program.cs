using Microsoft.AspNetCore.Authentication.Cookies; //1

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// ---- YENÝ EKLENEN KOD BLOÐU 1: API BAÐLANTISI (HttpClientFactory) ----
// Projemizin API ile konuþmasýný saðlayacak HttpClient'ý yapýlandýrýyoruz.
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Kendi API adresin (sonunda /api/ olduðundan emin ol)
    client.BaseAddress = new Uri("https://localhost:7180/api/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
// ---- YENÝ EKLENEN KISIM (SSL HATALARINI YOKSAYMAK ÝÇÝN) ----
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // SADECE Geliþtirme ortamýnda (development) kullanýlan
    // self-signed SSL sertifikalarýna güven.
    // DÝKKAT: Bu kodu production'a (canlýya) asla bu þekilde sürmeyin.
    handler.ServerCertificateCustomValidationCallback =
        (message, cert, chain, errors) => { return true; };

    return handler;
});// ---- KOD BLOÐU 1 SONU ----


// ---- YENÝ EKLENEN KOD BLOÐU 2: WEB SÝTESÝ GÝRÝÞ (Cookie Authentication) ----
// Kullanýcýnýn web sitesinde "login" olarak kalmasýný saðlamak için Cookie yapýlandýrmasý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true; // Cookie'ye Javascript'in eriþmesini engeller (Güvenlik)
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie 7 gün geçerli olsun
        options.LoginPath = "/Auth/Login"; // Kullanýcý giriþ yapmamýþsa bu sayfaya yönlendir
        options.AccessDeniedPath = "/Home/AccessDenied"; // Yetkisi olmayan bir sayfaya girmeye çalýþýrsa
        options.SlidingExpiration = true;
    });
// ---- KOD BLOÐU 2 SONU ----






var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <-- YENÝ EKLENDÝ

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
