using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Resources.Themes;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>
/// Theme, variant, font, text size and language. Every change is applied and stored at once by
/// <see cref="AppearanceService"/>; there is no save button.
/// </summary>
public partial class PersonalizacionViewModel : ObservableObject
{
    // The first fonts shown as chips; the rest are behind "+N".
    private const int VisibleFonts = 3;

    private static readonly string[] SwatchKeys = ["PageBackgroundColor", "SurfaceColor", "AccentColor", "PrimaryTextColor"];

    private readonly AppearanceService _appearance;
    private readonly Localizer _localizer = Localizer.Instance;

    // The "+N" chip, after the font chips: it shows the fonts hidden behind it.
    private readonly ChoiceItem _moreFonts;

    public PersonalizacionViewModel(AppearanceService appearance)
    {
        _appearance = appearance;

        Themes = ThemeCatalog.All
            .Select(t => new ChoiceItem(t.Id, () => _localizer[$"Theme_{t.Id}"]) { Swatches = ReadSwatches(t.Palettes[0]) })
            .ToList();
        Fonts = AppearanceOptions.Fonts
            .Select(f => new ChoiceItem(f.Name, () => $"Aa  {f.Name}") { FontFamily = f.Name, Description = () => f.Name })
            .ToList();
        var hiddenFonts = Fonts.Count - VisibleFonts;
        _moreFonts = new ChoiceItem(string.Empty, () => $"+{hiddenFonts}")
        {
            Description = () => _localizer.Format("Personalizacion_MoreFonts", hiddenFonts),
        };
        FontChips = [.. Fonts, _moreFonts];
        Scales = AppearanceOptions.Scales
            .Select((s, i) => new ChoiceItem(
                s.Scale.ToString(System.Globalization.CultureInfo.InvariantCulture),
                () => _localizer[$"Scale_{i}"])
            {
                SampleSize = Math.Round(AppearanceOptions.BodySize * s.Scale, 1),
            })
            .ToList();
        Languages = LanguageCatalog.All
            .Select(l => new ChoiceItem(l.Code, () => l.DisplayName))
            .ToList();

        UpdateSelection();
        _appearance.PropertyChanged += OnAppearanceChanged;
        _localizer.LanguageChanged += (_, _) => RefreshNames();
    }

    public IReadOnlyList<ChoiceItem> Themes { get; }

    /// <summary>The variants of the selected theme.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<ChoiceItem> Palettes { get; private set; } = [];

    /// <summary>The variant choice is shown only for a theme with more than one palette.</summary>
    public bool ShowPaletteChoice => Palettes.Count > 1;

    public IReadOnlyList<ChoiceItem> Fonts { get; }

    /// <summary>The font chips followed by the "+N" chip.</summary>
    public IReadOnlyList<ChoiceItem> FontChips { get; }

    public IReadOnlyList<ChoiceItem> Scales { get; }

    public IReadOnlyList<ChoiceItem> Languages { get; }

    private bool _allFontsShown;

    /// <summary>The name of the selected text size, shown under the size control.</summary>
    public string SelectedScaleName => Scales.FirstOrDefault(s => s.IsSelected)?.Name ?? string.Empty;

    [RelayCommand]
    private void SelectTheme(ChoiceItem item) => _appearance.SetTheme(item.Value);

    [RelayCommand]
    private void SelectPalette(ChoiceItem item) => _appearance.SetPalette(item.Value);

    [RelayCommand]
    private void SelectFont(ChoiceItem item)
    {
        if (item == _moreFonts)
        {
            _allFontsShown = true;
            UpdateFontVisibility();
            return;
        }

        _appearance.SetFontFamily(item.Value);
    }

    [RelayCommand]
    private void SelectScale(ChoiceItem item) =>
        _appearance.SetFontScale(double.Parse(item.Value, System.Globalization.CultureInfo.InvariantCulture));

    [RelayCommand]
    private void SelectLanguage(ChoiceItem item) => _appearance.SetLanguage(item.Value);

    [RelayCommand]
    private void ResetToDefaults() => _appearance.ResetToDefaults();

    private void OnAppearanceChanged(object? sender, PropertyChangedEventArgs e) => UpdateSelection();

    private void UpdateSelection()
    {
        foreach (var theme in Themes)
        {
            theme.IsSelected = theme.Value == _appearance.ThemeId;
        }

        var current = ThemeCatalog.Find(_appearance.ThemeId) ?? ThemeCatalog.Default;
        if (Palettes.Count != current.Palettes.Count || Palettes.Select(p => p.Value).Except(current.Palettes.Select(p => p.Id)).Any())
        {
            Palettes = current.Palettes.Select(p => new ChoiceItem(p.Id, () => _localizer[$"Palette_{p.Id}"])).ToList();
            OnPropertyChanged(nameof(ShowPaletteChoice));
        }

        foreach (var palette in Palettes)
        {
            palette.IsSelected = palette.Value == _appearance.PaletteId;
        }

        foreach (var font in Fonts)
        {
            font.IsSelected = font.Value == _appearance.FontFamilyId;
        }

        foreach (var scale in Scales)
        {
            scale.IsSelected = Math.Abs(double.Parse(scale.Value, System.Globalization.CultureInfo.InvariantCulture) - _appearance.FontScale) < 0.001;
        }

        foreach (var language in Languages)
        {
            language.IsSelected = language.Value == _appearance.LanguageCode;
        }

        UpdateFontVisibility();
        OnPropertyChanged(nameof(SelectedScaleName));
    }

    // The first fonts, the selected one and, after "+N", all of them.
    private void UpdateFontVisibility()
    {
        for (var i = 0; i < Fonts.Count; i++)
        {
            Fonts[i].IsShown = _allFontsShown || i < VisibleFonts || Fonts[i].IsSelected;
        }

        _moreFonts.IsShown = Fonts.Any(f => !f.IsShown);
    }

    private void RefreshNames()
    {
        foreach (var item in Themes.Concat(Palettes).Concat(FontChips).Concat(Scales).Concat(Languages))
        {
            item.RefreshName();
        }

        OnPropertyChanged(nameof(SelectedScaleName));
    }

    // A theme card shows its own colours, not the current palette's: read them from the theme's first palette.
    private static IReadOnlyList<Color> ReadSwatches(ThemePalette palette)
    {
        var dictionary = (ResourceDictionary)Activator.CreateInstance(palette.ResourceDictionaryType)!;
        return SwatchKeys
            .Select(key => dictionary.TryGetValue(key, out var value) && value is Color color ? color : Colors.Transparent)
            .ToList();
    }
}
