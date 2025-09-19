using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetCore_backend.DataAccess;
using NetCore_backend.Entities;
using NetCore_backend.Services.Abstractions;
using NetCore_backend.Services.Concretes;
using NetCore_backend.Settings;
using System.Text;

namespace NetCore_backend.Extensions
{
    public static class AuthServiceExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // --- 1. Ayarları Yapılandırma ve Doğrulama ---
            // 'JwtSettings' bölümünü 'appsettings.json'dan okuyup yapılandırıyoruz.
            var jwtSettingsSection = configuration.GetSection("Jwt");
            services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>()!; // '!' ile null olmadığını varsayıyoruz, aşağıda kontrol edeceğiz.

            // Fail-Fast: Gerekli ayarların varlığını uygulama başlarken kontrol et.
            if (string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key.Length < 32)
            {
                throw new InvalidOperationException("JWT Key must be configured and be at least 32 characters long.");
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured.");
            }

            // --- 2. Veritabanı ve Identity Servisleri ---
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Identity ayarlarını appsettings'ten okumak daha esnektir,
                // ama şimdilik burada bırakmak da kabul edilebilir.
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // --- 3. Bağımlılık Enjeksiyonu (DI) ---
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            // --- 4. JWT Kimlik Doğrulama Yapılandırması ---
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    // Middleware'e token'ı 'accessToken' adlı httpOnly cookie'den okumasını söylüyoruz.
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["accessToken"];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // --- 5. Yetkilendirme Servisi ---
            // Gelecekte rol bazlı politikalar eklemek için burayı genişletebiliriz.
            // Örn: options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            services.AddAuthorization();

            return services;
        }
    }
}
