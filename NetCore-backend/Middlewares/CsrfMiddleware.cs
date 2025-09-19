using Microsoft.AspNetCore.Cors.Infrastructure;
using NetCore_backend.Configurations;

namespace NetCore_backend.Middlewares
{
    public class CsrfMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CsrfMiddleware> _logger;

        // ILogger'ı dependency injection ile alıyoruz.
        public CsrfMiddleware(RequestDelegate next, ILogger<CsrfMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Eğer istek CSRF doğrulaması gerektirmiyorsa, bir sonraki adıma geç.
            if (!RequiresCsrfValidation(context))
            {
                await _next(context);
                return;
            }

            // CSRF token'larını al
            context.Request.Cookies.TryGetValue(CsrfConstants.CsrfCookieName, out var cookieToken);
            context.Request.Headers.TryGetValue(CsrfConstants.CsrfHeaderName, out var headerToken);

            // Token'ları doğrula
            if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || !cookieToken.Equals(headerToken))
            {
                _logger.LogWarning("CSRF token validation failed for user {User}. Path: {Path}",
                    context.User.Identity?.Name ?? "Anonymous",
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF token validation failed.");
                return;
            }

            // Doğrulama başarılı, bir sonraki adıma geç.
            await _next(context);
        }

        /// <summary>
        /// Bir HTTP isteğinin CSRF doğrulaması gerektirip gerektirmediğini belirler.
        /// </summary>
        private static bool RequiresCsrfValidation(HttpContext context)
        {
            // Kimliği doğrulanmamış kullanıcılar için kontrol gerekmez.
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // GET, HEAD, OPTIONS, TRACE gibi güvenli (side-effect'siz) metodlar için kontrol gerekmez.
            string method = context.Request.Method;
            if (HttpMethods.IsGet(method) ||
                HttpMethods.IsHead(method) ||
                HttpMethods.IsOptions(method) ||
                HttpMethods.IsTrace(method))
            {
                return false;
            }

            // Diğer tüm durumlar (POST, PUT, DELETE vb. ve kimliği doğrulanmış kullanıcı)
            // CSRF doğrulaması gerektirir.
            return true;
        }
    }

    // --- Extension Metodu ---
    public static class CsrfMiddlewareExtensions
    {
        public static IApplicationBuilder UseCsrfProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CsrfMiddleware>();
        }
    }
}
