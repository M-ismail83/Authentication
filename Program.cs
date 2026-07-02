using DotNetEnv;
using Authentication.Services;
using Authentication.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

var postgreConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");

var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION");

builder.Services.AddDbContext<dbContext>(options =>
    options.UseNpgsql(postgreConnectionString));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "AuthApp_";
});


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();