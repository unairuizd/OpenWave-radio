using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Resources.Themes;

namespace OpenWave.App.Services;

/// <summary>
/// Applies and remembers the theme, font family and text size. Styles read the colour, font and size
/// resources with DynamicResource, so every change here updates the whole app at once.
/// </summary>
public partial class AppearanceService : ObservableObject
{
    private const string ThemeKey = "appearance_theme";
    private const string FontKey = "appearance_font";
    private const string ScaleKey = "appearance_font_scale";
    private const double ScaleTolerance = 0.001;

    private ResourceDictionary? _themeDictionary;

    [ObservableProperty]
    public partial string ThemeId { get; private set; } = ThemeCatalog.DefaultId;

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
        ApplyTheme(ThemeCatalog.Find(ReadString(ThemeKey)) ?? ThemeCatalog.Find(ThemeCatalog.DefaultId)!);
        ApplyFont(FindFont(ReadString(FontKey)) ?? AppearanceOptions.DefaultFont);
        ApplyScale(FindScale(ReadDouble(ScaleKey)) ?? AppearanceOptions.DefaultScale);
    }

    public void SetTheme(string id)
    {
        var theme = ThemeCatalog.Find(id);
        if (theme is null)
        {
            return;
        }

        ApplyTheme(theme);
        Preferences.Default.Set(ThemeKey, theme.Id);
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
        SetTheme(ThemeCatalog.DefaultId);
        SetFontFamily(AppearanceOptions.DefaultFont);
        SetFontScale(AppearanceOptions.DefaultScale);
    }

    private void ApplyTheme(AppThemeDefinition theme)
    {
        var app = Application.Current!;
        var merged = app.Resources.MergedDictionaries;

        if (_themeDictionary is not null)
        {
            merged.Remove(_themeDictionary);
        }

        _themeDictionary = (ResourceDictionary)Activator.CreateInstance(theme.ResourceDictionaryType)!;
        merged.Add(_themeDictionary);

        // Native controls (inputs, pickers, dialogs) follow the theme's light or dark mode.
        app.UserAppTheme = theme.BaseTheme;
        ThemeId = theme.Id;
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
