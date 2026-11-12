using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OpenWave.Api;
using OpenWave.Api.Controllers;
using OpenWave.Api.Data;
using OpenWave.Api.Data.Entities;
using OpenWave.Api.Options;
using OpenWave.Api.Repositories;
using OpenWave.Api.Services;
using OpenWave.Contracts;

var builder = WebApplication.CreateBuilder(args);

// The API's own texts (ProblemDetails detail, content type labels) follow the request's Accept-Language.
// Spanish is the default and the fallback. To add a language: Resources/Messages.<code>.resx and its code here.
string[] supportedLanguages = ["es", "en"];
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<DatabaseExceptionFilter>();
        // Missing fields reach the services, which answer with specific Spanish messages.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Malformed or missing JSON bodies: a message in the request language instead of the default English one.
        // StationDto has required members, so a station without them never reaches the service.
        options.InvalidModelStateResponseFactory = context =>
        {
            var isStationBody = context.ActionDescriptor.Parameters.Any(p => p.ParameterType == typeof(StationDto));
            var services = context.HttpContext.RequestServices;
            var localizer = services.GetRequiredService<IStringLocalizer<Messages>>();
            var problem = services
                .GetRequiredService<ProblemDetailsFactory>()
                .CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    detail: localizer[isStationBody ? "InvalidStation" : "InvalidRequest"].Value);
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

// The catalogue option lists (60 minutes, see CatalogService) and the "on this day" items (until midnight UTC,
// see FactsService) are cached in memory.
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICatalogService, CatalogService>();

// Daily music facts: the curated lists are read once; Wikipedia is asked for today's music events with its own timeout.
builder.Services.AddSingleton(new FactsCatalog(builder.Environment.ContentRootPath, supportedLanguages));
builder.Services.Configure<WikimediaOptions>(builder.Configuration.GetSection(WikimediaOptions.SectionName));
builder.Services.AddHttpClient<IWikimediaClient, WikimediaClient>(client =>
{
    client.Timeout = WikimediaClient.RequestTimeout;
    client.DefaultRequestHeaders.UserAgent.ParseAdd(WikimediaClient.UserAgent);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<IFactsService, FactsService>();

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

app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(supportedLanguages[0])
        .AddSupportedCultures(supportedLanguages)
        .AddSupportedUICultures(supportedLanguages);
    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

app.UseAuthorization();

app.MapControllers();

app.Run();
