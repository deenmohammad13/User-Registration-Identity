using Microsoft.AspNetCore.Authorization;

namespace AuthECapp.Controllers
{
    public static class AuthorizationDemoEndpoints
    {
        public static IEndpointRouteBuilder MapAuthorizationDemoEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/AdminOnly", AdminOnly);

            app.MapGet("/AdminOrTeacher", [Authorize(Roles = "Admin,Teacher")] () =>
            { return "Admin Or Teacher"; });

            app.MapGet("/LibraryMemberOnly", [Authorize(Policy = "HasLibraryID")] () =>
            { return "Library Member Only"; });

            app.MapGet("/FemaleTeacherOnly", [Authorize(Roles = "Teacher", Policy = "FemaleOnly")] () =>
            { return "Applicable for Maternity Leave"; });

            app.MapGet("/FemaleUnder10",
                [Authorize(Policy = "Under10")]
                [Authorize(Policy = "FemaleOnly")] () =>
            { return "Fermale Under 10"; });

            return app;
        }

        [Authorize(Roles = "Admin")]
        private static string AdminOnly()
        {
            return "admin";
        }
    }
}
