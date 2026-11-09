namespace OpenWave.App.Services.Localization;

/// <summary>A language the app can be shown in: its culture code and its name written in that language.</summary>
public sealed record AppLanguage(string Code, string DisplayName);

/// <summary>
/// The registry of app languages, in the order the Idioma choice lists them. To add a language, add its
/// Resources/Strings/AppResources.&lt;code&gt;.resx file, the API's Messages.&lt;code&gt;.resx file and one line here.
/// </summary>
public static class LanguageCatalog
{
    public const string DefaultCode = "es";

    public static IReadOnlyList<AppLanguage> All { get; } =
    [
        new(DefaultCode, "Español"),
        new("en", "English"),
    ];

    public static AppLanguage Default => All[0];

    /// <summary>The registered language with this code, or null when it is missing or unknown.</summary>
    public static AppLanguage? Find(string? code) =>
        All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
}
