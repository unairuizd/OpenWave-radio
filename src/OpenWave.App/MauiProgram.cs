using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;
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
                // Each font's alias is its display name, which is also what Personalización stores.
                foreach (var font in AppearanceOptions.Fonts)
                {
                    fonts.AddFont(font.FileName, font.Name);
                    if (font.BoldFileName is not null)
                    {
                        fonts.AddFont(font.BoldFileName, font.BoldAlias);
                    }
                }
            });

        var apiSettings = new ApiSettings();
        // Every request states the app language, so the API answers its own texts in it.
        builder.Services.AddTransient<AcceptLanguageHandler>();
        builder.Services.AddSingleton(apiSettings);
        builder.Services.AddHttpClient<IStationService, StationService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AcceptLanguageHandler>();
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AcceptLanguageHandler>();
        builder.Services.AddHttpClient<FavoritesApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AcceptLanguageHandler>();
        builder.Services.AddHttpClient<FactsApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AcceptLanguageHandler>();
        builder.Services.AddHttpClient<CatalogApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        }).AddHttpMessageHandler<AcceptLanguageHandler>();
        builder.Services.AddSingleton<AppearanceService>();
        builder.Services.AddSingleton<PreferencesService>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<WelcomeService>();
        builder.Services.AddSingleton<FavoritesService>();
        // The daily facts, shared by the Inicio strip and Curiosidades; loaded only while logged in.
        builder.Services.AddSingleton<FactsService>();

        builder.Services.AddSingleton<MediaElementAudioPlayer>();
        builder.Services.AddSingleton<IAudioPlayer>(services => services.GetRequiredService<MediaElementAudioPlayer>());
        builder.Services.AddSingleton<PlaybackService>();

        builder.Services.AddSingleton<InicioViewModel>();
        builder.Services.AddSingleton<ReproductorViewModel>();
        builder.Services.AddSingleton<FavoritosViewModel>();

        builder.Services.AddSingleton<InicioPage>();
        builder.Services.AddSingleton<ReproductorPage>();
        builder.Services.AddSingleton<FavoritosPage>();

        // Ajustes. Personalización is a singleton too: its view model listens to the singleton AppearanceService,
        // so a new instance per visit would never be released.
        builder.Services.AddSingleton<AjustesViewModel>();
        builder.Services.AddSingleton<AjustesPage>();
        builder.Services.AddSingleton<PersonalizacionViewModel>();
        builder.Services.AddSingleton<PersonalizacionPage>();
        builder.Services.AddTransient<PreferenciasViewModel>();
        builder.Services.AddTransient<PreferenciasPage>();
        builder.Services.AddTransient<FiltroViewModel>();
        builder.Services.AddTransient<FiltroPage>();
        builder.Services.AddTransient<CuriosidadesViewModel>();
        builder.Services.AddTransient<CuriosidadesPage>();

        // A fresh login or registration form each time the flow opens.
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();

        // The welcome pop-up is shown once per device, from Inicio (WelcomeService keeps the flag).
        builder.Services.AddTransient<WelcomeViewModel>();
        builder.Services.AddTransient<WelcomePopup>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
