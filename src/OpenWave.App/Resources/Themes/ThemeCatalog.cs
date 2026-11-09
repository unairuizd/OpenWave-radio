namespace OpenWave.App.Resources.Themes;

/// <summary>
/// One colour variant of a theme: the resource dictionary with its colours and the light/dark mode that
/// native controls should use with it.
/// </summary>
/// <param name="Id">Stored in the device preferences; never shown.</param>
/// <param name="ResourceDictionaryType">A ResourceDictionary class defining every colour key.</param>
/// <param name="BaseTheme">Light or dark mode for native controls (inputs, pickers, dialogs).</param>
public sealed record ThemePalette(string Id, Type ResourceDictionaryType, AppTheme BaseTheme);

/// <summary>
/// A selectable theme: a layout (page templates, header, station card) and its palettes. Its name, and its
/// palettes' names, are the app resources Theme_&lt;id&gt; and Palette_&lt;id&gt;.
/// </summary>
/// <param name="Id">Stored in the device preferences; never shown.</param>
/// <param name="LayoutDictionaryType">A ResourceDictionary class defining every template key.</param>
/// <param name="Palettes">At least one palette; the first is used when no other applies.</param>
public sealed record AppThemeDefinition(string Id, Type LayoutDictionaryType, IReadOnlyList<ThemePalette> Palettes)
{
    /// <summary>
    /// The palette with that id, or null when this theme does not have it.
    /// </summary>
    public ThemePalette? FindPalette(string? id) => Palettes.FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// The palette to show for a stored variant: that palette when this theme has it, otherwise this theme's palette
    /// of the same light or dark kind, otherwise the first one. Palette ids are unique across the catalogue, so the
    /// kind of any stored id is known.
    /// </summary>
    public ThemePalette ChoosePalette(string? storedId)
    {
        var kind = ThemeCatalog.FindPalette(storedId)?.BaseTheme;
        return FindPalette(storedId) ?? Palettes.FirstOrDefault(p => p.BaseTheme == kind) ?? Palettes[0];
    }
}

/// <summary>
/// The themes the user can choose. To add one, create its palette dictionaries (the same colour keys as
/// <see cref="ModernoLightPalette"/>) and its layout dictionary (the same template keys as
/// <see cref="ModernoLayout"/>), and add one entry here. Palette ids must be unique across every theme, because a
/// stored variant is matched by its light or dark kind when another theme is chosen.
/// </summary>
public static class ThemeCatalog
{
    public const string DefaultId = "moderno";
    public const string DefaultPaletteId = "light";

    public static IReadOnlyList<AppThemeDefinition> All { get; } =
    [
        new(DefaultId, typeof(ModernoLayout),
        [
            new(DefaultPaletteId, typeof(ModernoLightPalette), AppTheme.Light),
            new("dark", typeof(ModernoDarkPalette), AppTheme.Dark),
        ]),
        new("tocadiscos", typeof(TocadiscosLayout),
        [
            new("standard", typeof(TocadiscosPalette), AppTheme.Dark),
        ]),
        new("valvulas", typeof(ValvulasLayout),
        [
            new("baquelita", typeof(ValvulasBaquelitaPalette), AppTheme.Light),
            new("nogal", typeof(ValvulasNogalPalette), AppTheme.Dark),
        ]),
        new("casete", typeof(CaseteLayout),
        [
            new("metal", typeof(CaseteMetalPalette), AppTheme.Dark),
        ]),
        new("boombox", typeof(BoomboxLayout),
        [
            new("cromado", typeof(BoomboxCromadoPalette), AppTheme.Dark),
        ]),
        new("mesadj", typeof(MesaDjLayout),
        [
            new("club", typeof(MesaDjClubPalette), AppTheme.Dark),
        ]),
        new("mp3", typeof(Mp3Layout),
        [
            new("azul", typeof(Mp3AzulPalette), AppTheme.Dark),
        ]),
        new("rueda", typeof(RuedaLayout),
        [
            new("blanco", typeof(RuedaBlancoPalette), AppTheme.Light),
            new("negro", typeof(RuedaNegroPalette), AppTheme.Dark),
        ]),
    ];

    public static AppThemeDefinition Default => Find(DefaultId)!;

    /// <summary>
    /// The theme with that id, or null when it is unknown.
    /// </summary>
    public static AppThemeDefinition? Find(string? id) => All.FirstOrDefault(t => t.Id == id);

    /// <summary>
    /// The palette with that id in any theme, or null when it is unknown.
    /// </summary>
    public static ThemePalette? FindPalette(string? id) =>
        All.SelectMany(t => t.Palettes).FirstOrDefault(p => p.Id == id);
}
