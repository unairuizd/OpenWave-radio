namespace OpenWave.App.Resources.Themes;

/// <summary>
/// One colour variant of a theme: the resource dictionary with its colours and the light/dark mode that
/// native controls should use with it.
/// </summary>
/// <param name="Id">Stored in the device preferences; never shown.</param>
/// <param name="DisplayName">Spanish name shown in Personalización.</param>
/// <param name="ResourceDictionaryType">A ResourceDictionary class defining every colour key.</param>
/// <param name="BaseTheme">Light or dark mode for native controls (inputs, pickers, dialogs).</param>
public sealed record ThemePalette(string Id, string DisplayName, Type ResourceDictionaryType, AppTheme BaseTheme);

/// <summary>
/// A selectable theme: a layout (page templates, header, bottom bar, station card) and its palettes.
/// </summary>
/// <param name="Id">Stored in the device preferences; never shown.</param>
/// <param name="DisplayName">Spanish name shown in Personalización.</param>
/// <param name="LayoutDictionaryType">A ResourceDictionary class defining every template key.</param>
/// <param name="Palettes">At least one palette; the first is used when no other applies.</param>
public sealed record AppThemeDefinition(string Id, string DisplayName, Type LayoutDictionaryType, IReadOnlyList<ThemePalette> Palettes)
{
    /// <summary>
    /// The palette with that id, or null when this theme does not have it.
    /// </summary>
    public ThemePalette? FindPalette(string? id) => Palettes.FirstOrDefault(p => p.Id == id);
}

/// <summary>
/// The themes the user can choose. To add one, create its palette dictionaries (the same colour keys as
/// <see cref="ModernoLightPalette"/>) and its layout dictionary (the same template keys as
/// <see cref="ModernoLayout"/>), and add one entry here.
/// </summary>
public static class ThemeCatalog
{
    public const string DefaultId = "moderno";
    public const string DefaultPaletteId = "light";

    public static IReadOnlyList<AppThemeDefinition> All { get; } =
    [
        new(DefaultId, "Moderno", typeof(ModernoLayout),
        [
            new(DefaultPaletteId, "Claro", typeof(ModernoLightPalette), AppTheme.Light),
            new("dark", "Oscuro", typeof(ModernoDarkPalette), AppTheme.Dark),
        ]),
        new("tocadiscos", "Tocadiscos", typeof(TocadiscosLayout),
        [
            new("standard", "Estándar", typeof(TocadiscosPalette), AppTheme.Dark),
        ]),
    ];

    public static AppThemeDefinition Default => Find(DefaultId)!;

    /// <summary>
    /// The theme with that id, or null when it is unknown.
    /// </summary>
    public static AppThemeDefinition? Find(string? id) => All.FirstOrDefault(t => t.Id == id);
}
