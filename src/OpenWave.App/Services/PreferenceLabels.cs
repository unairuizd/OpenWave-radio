using OpenWave.App.Services.Localization;

namespace OpenWave.App.Services;

/// <summary>
/// Text shown for saved preference values, using the same rules as the API's option labels,
/// so a saved value can be shown without loading the option lists.
/// </summary>
public static class PreferenceLabels
{
    public const string Separator = " · ";

    public static string Country(string value) => value;

    public static string Language(string value) => Capitalise(value);

    public static string Genre(string value) => Capitalise(value);

    // The fixed content types have a name in every app language; any other value is shown capitalised.
    public static string ContentType(string value)
    {
        var key = $"ContentType_{value.ToLowerInvariant()}";
        var label = Localizer.Instance[key];
        return label == key ? Capitalise(value) : label;
    }

    /// <summary>
    /// For example "Spain · Spanish · Jazz · Música"; only the values that are set.
    /// </summary>
    public static string Summary(UserPreferences preferences)
    {
        var parts = new List<string>();
        if (preferences.Country is { } country)
        {
            parts.Add(Country(country));
        }

        if (preferences.Language is { } language)
        {
            parts.Add(Language(language));
        }

        if (preferences.Genre is { } genre)
        {
            parts.Add(Genre(genre));
        }

        if (preferences.ContentType is { } contentType)
        {
            parts.Add(ContentType(contentType));
        }

        return string.Join(Separator, parts);
    }

    private static string Capitalise(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
