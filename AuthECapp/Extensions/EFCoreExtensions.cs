using AuthECapp.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthECapp.Extensions
{
    public static class EFCoreExtensions
    {
        public static IServiceCollection DependencyInjectons(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDBContext>(Options => 
            Options.UseSqlServer(config.GetConnectionString("DevConnection")));

            return services;
        }
    }
}
