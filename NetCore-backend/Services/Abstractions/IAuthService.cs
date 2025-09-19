using NetCore_backend.Entities;
using System.Security.Claims;

namespace NetCore_backend.Services.Abstractions
{
    public record AuthResult(bool Succeeded, AppUser? User, string AccessToken, string RefreshToken, string CsrfToken, string ErrorMessage = "");

    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
        Task<AuthResult> RefreshSessionAsync(string? currentRefreshToken);
        Task<bool> LogoutAsync(string? userId);
        Task<AppUser?> GetUserFromClaimsAsync(ClaimsPrincipal claimsPrincipal);
        Task<IList<string>> GetUserRolesAsync(AppUser user);
    }
}
