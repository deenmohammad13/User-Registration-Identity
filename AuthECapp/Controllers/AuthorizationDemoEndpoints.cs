using Microsoft.AspNetCore.Authorization;

namespace AuthECapp.Controllers
{
    public static class AuthorizationDemoEndpoints
    {
        public static IEndpointRouteBuilder MapAuthorizationDemoEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/adminOnly", AdminOnly);
            return app;
        }

        [Authorize(Roles = "Admin")]
        private static string AdminOnly()
        {
            return "admin";
        }
    }
}
