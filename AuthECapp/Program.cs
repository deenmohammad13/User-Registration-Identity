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
using AuthECapp.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerExplorer()
                .DependencyInjectons(builder.Configuration)
                .AppConfig(builder.Configuration)
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

app.MapGroup("/api")
   .MapIdentityUserEndpoints();

app.Run();

