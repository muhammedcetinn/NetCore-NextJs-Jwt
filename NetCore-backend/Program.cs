
using FluentValidation;
using NetCore_backend.DTOs;
using NetCore_backend.Extensions;
using NetCore_backend.Middlewares;
using NetCore_backend.Validations.AppUser;
using System;
var builder = WebApplication.CreateBuilder(args);

// --- Servisleri Konteynera Ekleme (Dependency Injection) ---

// 1. Temel API Servisleri
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Kendi yazdığımız Extension Metotları ile Servisleri Yapılandırma
builder.Services.AddCorsPolicies(builder.Configuration);
builder.Services.AddAuthServices(builder.Configuration);

/*
 * fluent alidaiton
 */

builder.Services.AddScoped<IValidator<AuthDtos.LoginRequest>, LoginRequestValidator>();

// --- Uygulama Pipeline'ını Yapılandırma ---
var app = builder.Build();

// Geliştirme ortamına özel pipeline yapılandırması
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Geliştirme sırasında daha detaylı hata sayfaları gösterir.
    app.UseDeveloperExceptionPage();
}
else
{
    // Production'da HTTPS yönlendirmesini zorunlu kıl.
    app.UseHttpsRedirection();
    // Production'da genel bir hata işleme sayfası kullanılabilir.
    // app.UseExceptionHandler("/Error");
    // app.UseHsts();
}

// Middleware'lerin sırası çok önemlidir!

// 1. Routing: Gelen isteğin hangi endpointe gideceğini belirler.
app.UseRouting();

// 2. CORS: Farklı origin'lerden gelen isteklere izin verir. Authentication'dan önce gelmelidir.
app.UseCorsPolicies();

// 3. Authentication: Gelen istekteki kimlik bilgilerini (cookie'deki token gibi) doğrular.
app.UseAuthentication();

// 4. Authorization: Kimliği doğrulanmış kullanıcının istenen kaynağa erişim yetkisi olup olmadığını kontrol eder.
app.UseAuthorization();

// 5. CSRF Koruması: Kimliği doğrulanmış kullanıcılar için ek bir güvenlik katmanı. Authorization'dan sonra gelmelidir.
app.UseCsrfProtection();

// 6. Endpoint'leri Eşleme: İsteği ilgili Controller metoduna yönlendirir.
app.MapControllers();

app.Run();