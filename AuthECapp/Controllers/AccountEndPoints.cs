﻿using AuthECapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthECapp.Controllers
{
    public static class AccountEndPoints
    {
        public static IEndpointRouteBuilder MapAccountEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/UserProfile", GetUserProfile);
            return app;
        }

        [Authorize]
        private static async Task<IResult> GetUserProfile(ClaimsPrincipal user,
            UserManager<AppUser> userManager)
        {
            string userID = user.Claims.First(x => x.Type == "UserID").Value;
            var userDetails = await userManager.FindByIdAsync(userID);
            return Results.Ok(new
            {
                Email = userDetails?.Email,
                FullName = userDetails?.FullName
            });
        }
    }
}
