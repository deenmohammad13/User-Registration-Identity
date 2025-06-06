using AuthECapp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthECapp.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityHandlersAndStores(this IServiceCollection services)  //Add service for Identity
        {
            services.AddIdentityApiEndpoints<AppUser>()
                    .AddEntityFrameworkStores<AppDBContext>();
            return services;
        }

        public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)  //Customize Validation
        {
            services.Configure<IdentityOptions>(Options =>
            {
                Options.Password.RequireDigit = false;
                Options.Password.RequireUppercase = false;
                Options.Password.RequireLowercase = false;
                Options.User.RequireUniqueEmail = true;
            });
            return services;
        }

        public static IServiceCollection AddIdentityAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(y =>                        //register jwt scheme with necessary configuration
                {
                    y.SaveToken = false;
                    y.TokenValidationParameters = new TokenValidationParameters     // JWT Validation
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(config["AppSettings:JWTSecret"]!)),

                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                });

            services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                });
            return services;
        }

        public static WebApplication AddIdentityAuthMiddlewares(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
