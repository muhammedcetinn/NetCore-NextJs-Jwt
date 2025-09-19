using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore_backend.Entities;
using NetCore_backend.Services.Abstractions;
using NetCore_backend.Validations.AppUser;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using NetCore_backend.DTOs;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace NetCore_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private IValidator<AuthDtos.LoginRequest> _validator;


        public AuthController(IAuthService authService, IConfiguration configuration, IValidator<AuthDtos.LoginRequest> validator)
        {
            _authService = authService;
            _configuration = configuration;
            _validator = validator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDtos.LoginRequest request)
        {
            ValidationResult validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Unauthorized(new { message = validationResult.Errors.ToList() });
            }
            // validaiton bitti
            var result = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            var userRoles = await _authService.GetUserRolesAsync(result.User);
            SetAuthCookies(result, request.RememberMe);
            return Ok(new AuthDtos.UserDto(result.User.Id, result.User.Email!, userRoles.ToList(), result.CsrfToken));
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshSession()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _authService.RefreshSessionAsync(refreshToken);

            if (!result.Succeeded)
            {
                ClearAuthCookies();
                return Unauthorized(new { message = result.ErrorMessage });
            }

            var userRoles = await _authService.GetUserRolesAsync(result.User);
            SetAuthCookies(result, result.User.RememberMe);
            return Ok(new AuthDtos.UserDto(result.User.Id, result.User.Email!, userRoles.ToList(), result.CsrfToken));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Servis katmanı zaten null kontrolü yapıyor, ama burada yapmak da iyidir.
            if (userId != null)
            {
                await _authService.LogoutAsync(userId);
            }

            ClearAuthCookies();
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Controller'daki 'User' nesnesi bir ClaimsPrincipal'dır.
            var user = await _authService.GetUserFromClaimsAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _authService.GetUserRolesAsync(user);
            var csrfToken = Request.Cookies["csrf-token"] ?? "";

            return Ok(new AuthDtos.UserDto(user.Id, user.Email!, roles.ToList(), csrfToken));
        }

        [Authorize]
        [HttpGet("check-auth")]
        public IActionResult CheckAuthenticationStatus() // async ve Task<IActionResult> kaldırıldı
        {
            // Bu metot içinde 'await' edilen bir işlem olmadığı için 'async' olmasına gerek yok.
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            return Ok(new { isAuthenticated = true, roles });
        }

        // --- Cookie Helper Metodları (Değişiklik yok) ---
        private void SetAuthCookies(AuthResult result, bool isPersistent)
        {
            var accessTokenDuration = _configuration.GetValue<double>("Jwt:AccessTokenDurationInMinutes");
            var refreshTokenDuration = _configuration.GetValue<int>("Jwt:RefreshTokenDurationInDays");

            SetCookie("accessToken", result.AccessToken, isPersistent, TimeSpan.FromMinutes(accessTokenDuration));
            SetCookie("refreshToken", result.RefreshToken, isPersistent, TimeSpan.FromDays(refreshTokenDuration));
            SetCookie("csrf-token", result.CsrfToken, isPersistent, TimeSpan.FromDays(refreshTokenDuration));
        }

        private void SetCookie(string key, string value, bool isPersistent, TimeSpan duration)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = isPersistent ? DateTime.UtcNow.Add(duration) : null
            };
            Response.Cookies.Append(key, value, cookieOptions);
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            Response.Cookies.Delete("csrf-token");
        }
    }

}
