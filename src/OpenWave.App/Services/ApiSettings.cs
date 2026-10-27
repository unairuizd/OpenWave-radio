namespace OpenWave.App.Services;

/// <summary>
/// Single source of truth for the backend base URL.
/// </summary>
public sealed class ApiSettings
{
    public const string PreferenceKey = "api_base_url";

    public ApiSettings()
    {
        BaseUrl = ResolveBaseUrl();
    }

    public string BaseUrl { get; }

    private static string ResolveBaseUrl()
    {
        var fallback = GetPlatformDefault();
        var configured = Preferences.Default.Get(PreferenceKey, fallback);

        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return fallback;
        }

        return configured.TrimEnd('/') + "/";
    }

    private static string GetPlatformDefault()
    {
#if ANDROID
        return "http://10.0.2.2:5080/";
#else
        return "http://localhost:5080/";
#endif
    }
}
