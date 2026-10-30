namespace OpenWave.App.Resources.Themes;

/// <summary>
/// A selectable theme: the resource dictionary with its colours and the light/dark mode that native
/// controls should use with it.
/// </summary>
/// <param name="Id">Stored in the device preferences; never shown.</param>
/// <param name="DisplayName">Spanish name shown in Personalización.</param>
/// <param name="ResourceDictionaryType">A ResourceDictionary class defining every colour key.</param>
/// <param name="BaseTheme">Light or dark mode for native controls (inputs, pickers, dialogs).</param>
public sealed record AppThemeDefinition(string Id, string DisplayName, Type ResourceDictionaryType, AppTheme BaseTheme);

/// <summary>
/// The themes the user can choose. To add one, create a dictionary with the same colour keys as
/// <see cref="WhiteTheme"/> and add one entry here.
/// </summary>
public static class ThemeCatalog
{
    public const string DefaultId = "white";

    public static IReadOnlyList<AppThemeDefinition> All { get; } =
    [
        new(DefaultId, "Blanco", typeof(WhiteTheme), AppTheme.Light),
        new("black", "Negro", typeof(BlackTheme), AppTheme.Dark),
    ];

    /// <summary>
    /// The theme with that id, or null when it is unknown.
    /// </summary>
    public static AppThemeDefinition? Find(string? id) => All.FirstOrDefault(t => t.Id == id);
}
