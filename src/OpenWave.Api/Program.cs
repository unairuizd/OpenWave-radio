using System.Net.Http.Headers;
using OpenWave.Api.Options;
using OpenWave.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
