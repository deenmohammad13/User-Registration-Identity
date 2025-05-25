using AuthECapp.Models;

namespace AuthECapp.Extensions
{
    public static class AppConfigExtensions
    {
        public static WebApplication ConfigureCORS(this WebApplication app, IConfiguration config)
        {
            app.UseCors();
            return app;
        }

        public static IServiceCollection AppConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AppSettings>(config.GetSection("AppSettings"));
            return services;
        }
    }
}
