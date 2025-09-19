using NetCore_backend.Entities;
using System.Security.Claims;

namespace NetCore_backend.Services.Abstractions
{
    public interface ITokenService
    {
        /// <summary>
        /// Belirtilen kullanıcı ve roller için bir JWT Access Token oluşturur.
        /// </summary>
        /// <param name="user">Token'ın oluşturulacağı kullanıcı.</param>
        /// <param name="roles">Token'a eklenecek roller.</param>
        /// <returns>Oluşturulan JWT string'i.</returns>
        string CreateAccessToken(AppUser user, IEnumerable<string> roles);

        /// <summary>
        /// Kriptografik olarak güvenli, benzersiz bir Refresh Token oluşturur.
        /// </summary>
        /// <returns>Base64 formatında rastgele bir string.</returns>
        string CreateRefreshToken();
    }
}
