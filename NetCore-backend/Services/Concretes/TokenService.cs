using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetCore_backend.Entities;
using NetCore_backend.Services.Abstractions;
using NetCore_backend.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NetCore_backend.Services.Concretes
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SymmetricSecurityKey _key; // Anahtarı bir kez oluşturup tekrar kullan

        public TokenService(IOptions<JwtSettings> jwtSettingsOptions)
        {
            _jwtSettings = jwtSettingsOptions.Value;
            // Anahtar ve imza bilgilerini constructor'da oluşturarak kod tekrarını önle.
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        }

        /// <inheritdoc />
        public string CreateAccessToken(AppUser user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                // Jti (JWT ID), her token'ın benzersiz olmasını sağlar ve token replay saldırılarını
                // önlemeye yardımcı olabilecek bir mekanizma için temel oluşturur.
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Rolleri 'role' claim'i olarak ekle
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenDurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        /// <inheritdoc />
        public string CreateRefreshToken()
        {
            // 64 byte'lık rastgele bir sayı, Base64'e çevrildiğinde yaklaşık 88 karakterlik
            // son derece güvenli bir string oluşturur.
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
