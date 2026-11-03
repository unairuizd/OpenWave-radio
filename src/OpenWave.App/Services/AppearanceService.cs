using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Resources.Themes;

namespace OpenWave.App.Services;

/// <summary>
/// Applies and remembers the theme, its palette (variant), the font family and the text size. Styles and
/// layouts read the colour, font and size resources with DynamicResource, so every change here updates the
/// whole app at once.
/// </summary>
public partial class AppearanceService : ObservableObject
{
    private const string ThemeKey = "appearance_theme";
    private const string PaletteKey = "appearance_theme_variant";
    private const string FontKey = "appearance_font";
    private const string ScaleKey = "appearance_font_scale";
    private const double ScaleTolerance = 0.001;

    // The same values as MinTouchTarget and the IconButton padding in Styles.xaml.
    private const double MinTouchTarget = 48;
    private const double IconPadding = 12;

    // Theme ids stored by the previous version, which had one dictionary per colour scheme.
    private static readonly Dictionary<string, (string ThemeId, string PaletteId)> LegacyThemes = new()
    {
        ["white"] = (ThemeCatalog.DefaultId, "light"),
        ["black"] = (ThemeCatalog.DefaultId, "dark"),
    };

    private ResourceDictionary? _paletteDictionary;
    private ResourceDictionary? _layoutDictionary;

    // True while a theme is being applied.
    private bool _applying;

    [ObservableProperty]
    public partial string ThemeId { get; private set; } = ThemeCatalog.DefaultId;

    [ObservableProperty]
    public partial string PaletteId { get; private set; } = ThemeCatalog.DefaultPaletteId;

    [ObservableProperty]
    public partial string FontFamilyId { get; private set; } = AppearanceOptions.DefaultFont;

    [ObservableProperty]
    public partial double FontScale { get; private set; } = AppearanceOptions.DefaultScale;

    /// <summary>
    /// Applies the stored choices. Call once at startup, before the first page is created.
    /// Missing, unknown or corrupt values fall back to the defaults.
    /// </summary>
    public void Initialize()
    {
        var (theme, palette) = ReadStoredTheme();
        ApplyTheme(theme, palette);
        ApplyFont(FindFont(ReadString(FontKey)) ?? AppearanceOptions.DefaultFont);
        ApplyScale(FindScale(ReadDouble(ScaleKey)) ?? AppearanceOptions.DefaultScale);
    }

    /// <summary>
    /// Switches to another theme, keeping the stored variant, or its light or dark kind, when the new theme has it.
    /// </summary>
    public void SetTheme(string id)
    {
        var theme = ThemeCatalog.Find(id);
        if (theme is null || _applying)
        {
            return;
        }

        // The variant last chosen is remembered even while a single-palette theme is shown, so it comes back with
        // its theme.
        var palette = theme.ChoosePalette(ReadString(PaletteKey) ?? PaletteId);
        ApplyTheme(theme, palette);
        StoreTheme(theme, palette);
    }

    /// <summary>
    /// Switches to another variant of the current theme.
    /// </summary>
    public void SetPalette(string id)
    {
        var theme = ThemeCatalog.Find(ThemeId) ?? ThemeCatalog.Default;
        var palette = theme.FindPalette(id);
        if (palette is null || _applying)
        {
            return;
        }

        ApplyTheme(theme, palette);
        StoreTheme(theme, palette);
    }

    public void SetFontFamily(string name)
    {
        var font = FindFont(name);
        if (font is null)
        {
            return;
        }

        ApplyFont(font);
        Preferences.Default.Set(FontKey, font);
    }

    public void SetFontScale(double scale)
    {
        var preset = FindScale(scale);
        if (preset is null)
        {
            return;
        }

        ApplyScale(preset.Value);
        Preferences.Default.Set(ScaleKey, preset.Value);
    }

    public void ResetToDefaults()
    {
        var theme = ThemeCatalog.Default;
        var palette = theme.FindPalette(ThemeCatalog.DefaultPaletteId) ?? theme.Palettes[0];
        ApplyTheme(theme, palette);
        StoreTheme(theme, palette);
        SetFontFamily(AppearanceOptions.DefaultFont);
        SetFontScale(AppearanceOptions.DefaultScale);
    }

    /// <summary>
    /// The stored theme and palette. Values of the previous version are migrated and written back;
    /// an unknown theme becomes the default, and a palette the theme does not have becomes its palette of the same
    /// light or dark kind, or its first one.
    /// </summary>
    private static (AppThemeDefinition Theme, ThemePalette Palette) ReadStoredTheme()
    {
        var themeId = ReadString(ThemeKey);
        var paletteId = ReadString(PaletteKey);

        if (themeId is not null && LegacyThemes.TryGetValue(themeId, out var migrated))
        {
            (themeId, paletteId) = migrated;
            Preferences.Default.Set(ThemeKey, themeId);
            Preferences.Default.Set(PaletteKey, paletteId);
        }

        var theme = ThemeCatalog.Find(themeId);
        if (theme is null)
        {
            theme = ThemeCatalog.Default;
            paletteId = ThemeCatalog.DefaultPaletteId;
        }

        return (theme, theme.ChoosePalette(paletteId));
    }

    // A theme with a single palette leaves the stored variant alone, so the other theme's variant is kept.
    private static void StoreTheme(AppThemeDefinition theme, ThemePalette palette)
    {
        Preferences.Default.Set(ThemeKey, theme.Id);
        if (theme.Palettes.Count > 1)
        {
            Preferences.Default.Set(PaletteKey, palette.Id);
        }
    }

    private void ApplyTheme(AppThemeDefinition theme, ThemePalette palette)
    {
        var app = Application.Current!;
        var merged = app.Resources.MergedDictionaries;

        if (_paletteDictionary is not null)
        {
            merged.Remove(_paletteDictionary);
        }

        _paletteDictionary = (ResourceDictionary)Activator.CreateInstance(palette.ResourceDictionaryType)!;
        merged.Add(_paletteDictionary);

        // The layout (page templates, header, bottom bar, station card) only changes with the theme, not with the
        // variant, so switching Claro/Oscuro never rebuilds the pages.
        if (_layoutDictionary?.GetType() != theme.LayoutDictionaryType)
        {
            if (_layoutDictionary is not null)
            {
                merged.Remove(_layoutDictionary);
            }

            _layoutDictionary = (ResourceDictionary)Activator.CreateInstance(theme.LayoutDictionaryType)!;
            merged.Add(_layoutDictionary);
        }

        // Native controls (inputs, pickers, dialogs) follow the palette's light or dark mode.
        app.UserAppTheme = palette.BaseTheme;

        // The palette first: listeners that rebuild their variant list when the theme changes then already see the
        // new variant. Selections echoed back while this runs are ignored (see SetTheme and SetPalette).
        _applying = true;
        try
        {
            PaletteId = palette.Id;
            ThemeId = theme.Id;
        }
        finally
        {
            _applying = false;
        }
    }

    private void ApplyFont(string name)
    {
        var resources = Application.Current!.Resources;
        resources["AppFontFamily"] = name;
        resources["AppFontFamilyBold"] = AppearanceOptions.Fonts.First(f => f.Name == name).BoldAlias;
        FontFamilyId = name;
    }

    private void ApplyScale(double scale)
    {
        var resources = Application.Current!.Resources;
        resources["FontSizeCaption"] = Math.Round(AppearanceOptions.CaptionSize * scale, 1);
        resources["FontSizeBody"] = Math.Round(AppearanceOptions.BodySize * scale, 1);
        resources["FontSizeSubtitle"] = Math.Round(AppearanceOptions.SubtitleSize * scale, 1);
        resources["FontSizeTitle"] = Math.Round(AppearanceOptions.TitleSize * scale, 1);
        // Icons are as wide as a title letter is high, plus their padding, so they scale with the text.
        resources["IconButtonSize"] = Math.Max(MinTouchTarget, Math.Round(AppearanceOptions.TitleSize * scale, 1) + 2 * IconPadding);
        FontScale = scale;
    }

    private static string? FindFont(string? name) =>
        AppearanceOptions.Fonts.FirstOrDefault(f => f.Name == name)?.Name;

    private static double? FindScale(double? scale) =>
        AppearanceOptions.Scales.FirstOrDefault(s => scale is not null && Math.Abs(s.Scale - scale.Value) < ScaleTolerance)?.Scale;

    // A stored value of the wrong type throws when read: treat it as missing.
    private static string? ReadString(string key)
    {
        try
        {
            return Preferences.Default.Get<string?>(key, null);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static double? ReadDouble(string key)
    {
        try
        {
            return Preferences.Default.ContainsKey(key) ? Preferences.Default.Get(key, AppearanceOptions.DefaultScale) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
