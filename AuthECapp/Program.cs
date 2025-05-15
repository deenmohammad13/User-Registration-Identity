using AuthECapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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

app.Run();

public class UserRegistrationModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}