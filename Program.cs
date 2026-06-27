using Authentication.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PostgresBaglantim");
builder.Services.AddDbContext<Authentication.Data.dbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisBaglantim");
    options.InstanceName = "AuthApp_";
});


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();