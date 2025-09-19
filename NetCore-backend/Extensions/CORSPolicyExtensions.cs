namespace NetCore_backend.Extensions
{
    public static class CORSPolicyExtensions
    {
        private const string CorsPolicyName = "AllowConfiguredOrigins";

        /// <summary>
        /// 'appsettings.json' dosyasından okunan CORS politikalarını servislere ekler.
        /// </summary>
        public static IServiceCollection AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                // 'Cors:AllowedOrigins' bölümünden URL'leri oku.
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                     ?? configuration.GetValue<string>("Cors:AllowedOrigins")?.Split(',')
                                     ?? Array.Empty<string>();

                // Fail-Fast: Eğer hiçbir origin belirtilmemişse, geliştiriciyi uyar.
                if (allowedOrigins.Length == 0)
                {
                    // Geliştirme ortamında bu bir uyarı olabilir, production'da ise bir exception fırlatılabilir.
                    Console.WriteLine("Warning: No CORS origins configured in appsettings.json. Cross-origin requests may fail.");
                }

                Console.WriteLine($"CORS policy '{CorsPolicyName}' configured for origins: {string.Join(", ", allowedOrigins)}");

                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Yapılandırılmış CORS politikasını uygulama pipeline'ına ekler.
        /// </summary>
        public static IApplicationBuilder UseCorsPolicies(this IApplicationBuilder app)
        {
            app.UseCors(CorsPolicyName);
            return app;
        }
    }
}
