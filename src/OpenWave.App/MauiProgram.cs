using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenWave.App.Services;
using OpenWave.App.ViewModels;
using OpenWave.App.Views;

namespace OpenWave.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            // No Android foreground service: background playback is out of scope.
            .UseMauiCommunityToolkitMediaElement(false)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var apiSettings = new ApiSettings();
        builder.Services.AddSingleton(apiSettings);
        builder.Services.AddHttpClient<IStationService, StationService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        builder.Services.AddHttpClient<FavoritesApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<FavoritesService>();

        builder.Services.AddSingleton<MediaElementAudioPlayer>();
        builder.Services.AddSingleton<IAudioPlayer>(services => services.GetRequiredService<MediaElementAudioPlayer>());
        builder.Services.AddSingleton<PlaybackService>();

        builder.Services.AddSingleton<InicioViewModel>();
        builder.Services.AddSingleton<ReproductorViewModel>();
        builder.Services.AddSingleton<FavoritosViewModel>();

        builder.Services.AddSingleton<InicioPage>();
        builder.Services.AddSingleton<ReproductorPage>();
        builder.Services.AddSingleton<FavoritosPage>();

        // A fresh login or registration form each time the flow opens.
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
