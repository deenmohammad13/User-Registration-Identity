using AuthECapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthECapp.Controllers
{
    public class UserRegistrationModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public static class IdentityUserEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/signup", CreateUser);

            app.MapPost("/signin", SignIn);
            return app;
        }

        [AllowAnonymous]
        private static async Task<IResult> CreateUser(UserManager<AppUser> userManager,
                                [FromBody] UserRegistrationModel userRegistrationModel)
                    {
                        AppUser user = new AppUser()
                        {
                            UserName = userRegistrationModel.Email, //assign email as username
                            Email = userRegistrationModel.Email,
                            FullName = userRegistrationModel.FullName
                        };

                        var result = await userManager.CreateAsync(user, userRegistrationModel.Password);
                        if (result.Succeeded)
                            return Results.Ok(result);
                        else
                            return Results.BadRequest(result);
                    }

        [AllowAnonymous]
        private static async Task<IResult> SignIn(UserManager<AppUser> userManager,            //argument for managing user
                                                 [FromBody] LoginModel loginModel,
                                                 IOptions<AppSettings> appSettings)
                {
                    var user = await userManager.FindByEmailAsync(loginModel.Email);    //verify by email
                    if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))  //check the password and genarate the jwt
                    {
                        var signInKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(appSettings.Value.JWTSecret));   // save the secret key

                        var tokenDescriptor = new SecurityTokenDescriptor   // add information or claim to the payload
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                            new Claim("UserID", user.Id.ToString())     // customize claim for payload, can be add more
                            }),

                            Expires = DateTime.UtcNow.AddDays(10),    //expiration time for token

                            SigningCredentials = new SigningCredentials(signInKey,
                                SecurityAlgorithms.HmacSha256)  //specify the security key and algorithm
                        };

                        var tokenHandler = new JwtSecurityTokenHandler();               //creating the tokenhandler
                        var securityToken = tokenHandler.CreateToken(tokenDescriptor);  //creating the token
                        var token = tokenHandler.WriteToken(securityToken);            //token in serialized format which have to pass
                        return Results.Ok(new { token });                               // finally return the token in object form
                    }

                    else
                        return Results.BadRequest(new { message = "Username or Password is incorrect" });
                }
    }
}
