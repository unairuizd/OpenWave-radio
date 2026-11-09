using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace OpenWave.App.Services.Localization;

/// <summary>
/// Gives every text of the app in the current language. XAML binds to the indexer through
/// <see cref="TranslateExtension"/>, so changing the language updates every open page at once.
/// Spanish is the neutral resource: a key missing from another language falls back to its Spanish text.
/// </summary>
public sealed class Localizer : INotifyPropertyChanged
{
    private static readonly ResourceManager Resources =
        new("OpenWave.App.Resources.Strings.AppResources", typeof(Localizer).Assembly);

    private CultureInfo _culture = CultureInfo.GetCultureInfo(LanguageCatalog.DefaultCode);

    private Localizer()
    {
    }

    public static Localizer Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raised after the language changed and every binding was refreshed.</summary>
    public event EventHandler? LanguageChanged;

    /// <summary>The code of the current language, as registered in <see cref="LanguageCatalog"/>.</summary>
    public string LanguageCode { get; private set; } = LanguageCatalog.DefaultCode;

    /// <summary>The text for the key in the current language; the key itself when no language defines it.</summary>
    public string this[string key] => Resources.GetString(key, _culture) ?? key;

    /// <summary>The text for the key, formatted with the arguments in the current language's culture.</summary>
    public string Format(string key, params object?[] args) => string.Format(_culture, this[key], args);

    /// <summary>
    /// Switches to a registered language. An unknown code switches to Spanish. Numbers and dates the app
    /// formats follow the new language too.
    /// </summary>
    public void SetLanguage(string? code)
    {
        var language = LanguageCatalog.Find(code) ?? LanguageCatalog.Default;
        _culture = CultureInfo.GetCultureInfo(language.Code);
        CultureInfo.CurrentUICulture = _culture;
        CultureInfo.CurrentCulture = _culture;
        CultureInfo.DefaultThreadCurrentUICulture = _culture;
        CultureInfo.DefaultThreadCurrentCulture = _culture;
        LanguageCode = language.Code;

        // A null name refreshes every binding to this object, whatever key it reads.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
