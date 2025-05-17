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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add service for Identity
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<AppDBContext>();

//Customize Validation
builder.Services.Configure<IdentityOptions>(Options =>
    {
        Options.Password.RequireDigit = false;
        Options.Password.RequireUppercase = false;
        Options.Password.RequireLowercase = false;
        Options.User.RequireUniqueEmail = true;
    });

// Dependency Injection
builder.Services.AddDbContext<AppDBContext>(
    Options => Options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

// services and configuration for authentication
builder.Services.AddAuthentication(x =>                           
{
    x.DefaultAuthenticateScheme =                             //authentication options     
    x.DefaultChallengeScheme =
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; //used as the fallback default scheme for all other defaults

})
    .AddJwtBearer(y =>                        //register jwt scheme with necessary configuration
    {
        y.SaveToken = false;
        y.TokenValidationParameters = new TokenValidationParameters     // JWT Validation
        {
            ValidateIssuer = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JWTSecret"]!))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//Asp.NET Middlewares
# region configure CORS
app.UseCors();
#endregion
app.UseAuthentication();
app.UseAuthorization();

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