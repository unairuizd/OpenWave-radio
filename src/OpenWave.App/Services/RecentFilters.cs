namespace OpenWave.App.Services;

/// <summary>The filters of Preferencias that have a selection page with recent values.</summary>
public enum FilterKind
{
    Country,
    Language,
    Genre,
}

/// <summary>
/// The last values the user saved for each filter, newest first, at most <see cref="MaxCount"/>, without "Todos" and
/// without repeats. Stored on the device only, never linked to the account.
/// </summary>
public static class RecentFilters
{
    public const int MaxCount = 5;

    // One line per value; filter values never contain a line break.
    private const char Separator = '\n';

    public static IReadOnlyList<string> Get(FilterKind kind)
    {
        try
        {
            var stored = Preferences.Default.Get<string?>(Key(kind), null);
            return string.IsNullOrEmpty(stored)
                ? []
                : stored.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Take(MaxCount).ToList();
        }
        catch (Exception)
        {
            // A stored value of the wrong type: treat it as no recent values.
            return [];
        }
    }

    /// <summary>Puts a saved value first. "Todos" (null) is not remembered.</summary>
    public static void Add(FilterKind kind, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var values = Get(kind)
            .Where(v => !string.Equals(v, value, StringComparison.OrdinalIgnoreCase))
            .Prepend(value)
            .Take(MaxCount);
        Preferences.Default.Set(Key(kind), string.Join(Separator, values));
    }

    private static string Key(FilterKind kind) => $"recent_{kind.ToString().ToLowerInvariant()}";
}
