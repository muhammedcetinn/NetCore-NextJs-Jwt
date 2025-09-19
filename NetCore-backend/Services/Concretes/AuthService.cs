using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetCore_backend.Entities;
using NetCore_backend.Services.Abstractions;
using System.Security.Claims;
using System.Security.Cryptography;

namespace NetCore_backend.Services.Concretes
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, ITokenService tokenService, IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return new AuthResult(false, null, "", "", "", "Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateAccessToken(user, roles);
            var refreshToken = _tokenService.CreateRefreshToken();

            var refreshTokenExpiryTime = rememberMe
                ? DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenDurationInDays"))
                : DateTime.UtcNow.AddHours(12);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiryTime;
            user.RememberMe = rememberMe;
            await _userManager.UpdateAsync(user);

            var csrfToken = GenerateCsrfToken(); // Basit bir CSRF token üretimi
            return new AuthResult(true, user, accessToken, refreshToken, csrfToken);
        }

        public async Task<AuthResult> RefreshSessionAsync(string? currentRefreshToken)
        {
            if (string.IsNullOrEmpty(currentRefreshToken))
            {
                return new AuthResult(false, null, "", "", "", "Refresh token is required.");
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == currentRefreshToken);
            if (user == null)
            {
                return new AuthResult(false, null, "", "", "", "Invalid refresh token.");
            }
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResult(false, null, "", "", "", "Refresh token has expired.");
            }

            // "Beni Hatırla" durumunu veritabanından oku
            bool isPersistent = user.RememberMe;

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.CreateAccessToken(user, roles);
            var newRefreshToken = _tokenService.CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = isPersistent
                ? DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenDurationInDays"))
                : DateTime.UtcNow.AddHours(12);
            await _userManager.UpdateAsync(user);

            var csrfToken = GenerateCsrfToken();
            return new AuthResult(true, user, newAccessToken, newRefreshToken, csrfToken);
        }

        public async Task<bool> LogoutAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return true; // Kullanıcı yoksa zaten çıkış yapılmıştır.

            user.RefreshToken = null;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// Bir ClaimsPrincipal'dan (genellikle Controller'daki User nesnesi) kullanıcıyı bulur.
        /// </summary>
        public async Task<AppUser?> GetUserFromClaimsAsync(ClaimsPrincipal claimsPrincipal)
        {
            // NameIdentifier claim'i, JWT'yi oluştururken 'sub' olarak set ettiğimiz kullanıcı ID'sini içerir.
            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return await _userManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Belirtilen kullanıcının rollerini bir liste olarak döndürür.
        /// </summary>
        public async Task<IList<string>> GetUserRolesAsync(AppUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        private string GenerateCsrfToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
