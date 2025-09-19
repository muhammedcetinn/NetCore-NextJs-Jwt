namespace NetCore_backend.DTOs
{
    public class AuthDtos
    {
        public record LoginRequest(string Email, string Password, bool RememberMe);
        public record UserDto(string Id, string Email, List<string> Roles, string CsrfToken);
    }
}
