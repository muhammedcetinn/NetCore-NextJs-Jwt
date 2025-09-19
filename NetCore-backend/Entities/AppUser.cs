using Microsoft.AspNetCore.Identity;

namespace NetCore_backend.Entities
{
    public class AppUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool RememberMe { get; set; }
    }
}
