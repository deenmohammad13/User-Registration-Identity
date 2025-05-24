using AuthECapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using AuthECapp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerExplorer()
                .DependencyInjectons(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddIdentityAuthenticationAndAuthorization(builder.Configuration);

var app = builder.Build();

app.ConfigureSwaggerExplorer()
    .ConfigureCORS(builder.Configuration)
    .AddIdentityAuthMiddlewares(); ;

app.UseHttpsRedirection();

app.MapControllers();

app.MapGroup("/api")
    .MapIdentityApi<AppUser>();

app.MapPost("/api/signup", async (
    UserManager<AppUser> userManager,
    [FromBody] UserRegistrationModel userRegistrationModel 
    ) =>
    {
        AppUser user = new AppUser()
        {
            UserName = userRegistrationModel.Email, //assign email as username
            Email = userRegistrationModel.Email,
            FullName = userRegistrationModel.FullName
        };

       var result = await userManager.CreateAsync(user,userRegistrationModel.Password);
        if (result.Succeeded)
            return Results.Ok(result);
        else
            return Results.BadRequest(result);

});

app.MapPost("/api/signin", async (
    UserManager<AppUser> userManager,            //argument for managing user
    [FromBody] LoginModel loginModel
    ) => 
    {
        var user = await userManager.FindByEmailAsync(loginModel.Email);    //verify by email
        if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))  //check the password and genarate the jwt
        {
            var signInKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JWTSecret"]!));   // save the secret key

            var tokenDescriptor = new SecurityTokenDescriptor   // add information or claim to the payload
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("User Id", user.Id.ToString())     // customize claim for payload, can be add more
                }),
                
                Expires = DateTime.UtcNow.AddDays(10),    //expiration time for token
                
                SigningCredentials = new SigningCredentials(signInKey,
                    SecurityAlgorithms.HmacSha256)  //specify the security key and algorithm
            };

            var tokenHandler = new JwtSecurityTokenHandler();               //creating the tokenhandler
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);  //creating the token
            var token =  tokenHandler.WriteToken(securityToken);            //token in serialized format which have to pass
            return Results.Ok(new { token });                               // finally return the token in object form
        }

        else
            return Results.BadRequest(new { message = "Username or Password is incorrect"});

});

app.Run();

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