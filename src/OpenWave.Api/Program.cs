using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenWave.Api.Controllers;
using OpenWave.Api.Data;
using OpenWave.Api.Data.Entities;
using OpenWave.Api.Options;
using OpenWave.Api.Repositories;
using OpenWave.Api.Services;
using OpenWave.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<DatabaseExceptionFilter>();
        // Missing fields reach the services, which answer with specific Spanish messages.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Malformed or missing JSON bodies: a Spanish message instead of the default English one.
        // StationDto has required members, so a station without them never reaches the service.
        options.InvalidModelStateResponseFactory = context =>
        {
            var isStationBody = context.ActionDescriptor.Parameters.Any(p => p.ParameterType == typeof(StationDto));
            var problem = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>()
                .CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: isStationBody ? "Los datos de la emisora no son válidos." : "La solicitud no es válida.");
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RadioBrowserOptions>(builder.Configuration.GetSection(RadioBrowserOptions.SectionName));
builder.Services.AddHttpClient<IRadioBrowserClient, RadioBrowserClient>(client =>
{
    // The client enforces a per-mirror timeout itself.
    client.Timeout = Timeout.InfiniteTimeSpan;
    client.DefaultRequestHeaders.UserAgent.ParseAdd(RadioBrowserClient.UserAgent);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IStationService, StationService>();

// The schema is created only by database/schema.sql: no migrations and no EnsureCreated.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Default'.");
builder.Services.AddDbContext<OpenWaveDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddSingleton<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
