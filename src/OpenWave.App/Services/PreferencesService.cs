namespace OpenWave.App.Services;

/// <summary>
/// The saved default station filters. A null value means "Todos" (no filter).
/// </summary>
public sealed record UserPreferences(string? Country, string? Language, string? Genre, string? ContentType)
{
    public static UserPreferences None { get; } = new(null, null, null, null);

    public bool HasAny => Country is not null || Language is not null || Genre is not null || ContentType is not null;
}

/// <summary>
/// Stores the Preferencias on the device (never in the database, independent of the session) and
/// whether Inicio applies them during this run of the app.
/// </summary>
public sealed class PreferencesService
{
    private const string CountryKey = "pref_country";
    private const string LanguageKey = "pref_language";
    private const string GenreKey = "pref_genre";
    private const string ContentTypeKey = "pref_content_type";

    private bool _applyInInicio = true;

    public PreferencesService()
    {
        Current = new UserPreferences(Read(CountryKey), Read(LanguageKey), Read(GenreKey), Read(ContentTypeKey));
    }

    /// <summary>
    /// Raised when the saved values or <see cref="ApplyInInicio"/> change.
    /// </summary>
    public event EventHandler? Changed;

    public UserPreferences Current { get; private set; }

    /// <summary>
    /// Whether Inicio sends the saved filters. Kept in memory only: true at every app start.
    /// </summary>
    public bool ApplyInInicio
    {
        get => _applyInInicio;
        set
        {
            if (_applyInInicio != value)
            {
                _applyInInicio = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void Save(UserPreferences preferences)
    {
        Write(CountryKey, preferences.Country);
        Write(LanguageKey, preferences.Language);
        Write(GenreKey, preferences.Genre);
        Write(ContentTypeKey, preferences.ContentType);
        Current = preferences;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset() => Save(UserPreferences.None);

    private static void Write(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Preferences.Default.Remove(key);
        }
        else
        {
            Preferences.Default.Set(key, value);
        }
    }

    // A stored value of the wrong type throws when read: treat it as not saved.
    private static string? Read(string key)
    {
        try
        {
            var value = Preferences.Default.Get<string?>(key, null);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
