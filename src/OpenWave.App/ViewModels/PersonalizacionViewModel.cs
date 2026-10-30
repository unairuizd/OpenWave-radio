using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Resources.Themes;
using OpenWave.App.Services;

namespace OpenWave.App.ViewModels;

/// <summary>
/// Theme, font and text size selectors. Every change is applied and stored at once by
/// <see cref="AppearanceService"/>; there is no save button.
/// </summary>
public partial class PersonalizacionViewModel : ObservableObject
{
    private readonly AppearanceService _appearance;

    public PersonalizacionViewModel(AppearanceService appearance)
    {
        _appearance = appearance;
        SelectedThemeId = appearance.ThemeId;
        SelectedFont = appearance.FontFamilyId;
        SelectedScale = appearance.FontScale;

        // Keeps the selection in step when the appearance changes elsewhere (for example, the reset).
        _appearance.PropertyChanged += OnAppearanceChanged;
    }

    public IReadOnlyList<AppThemeDefinition> Themes => ThemeCatalog.All;

    public IReadOnlyList<FontOption> Fonts => AppearanceOptions.Fonts;

    public IReadOnlyList<FontScaleOption> Scales => AppearanceOptions.Scales;

    // Radio button values arrive as object, so the selections are object-typed.
    [ObservableProperty]
    public partial object? SelectedThemeId { get; set; }

    [ObservableProperty]
    public partial object? SelectedFont { get; set; }

    [ObservableProperty]
    public partial object? SelectedScale { get; set; }

    partial void OnSelectedThemeIdChanged(object? value)
    {
        if (value is string id && id != _appearance.ThemeId)
        {
            _appearance.SetTheme(id);
        }
    }

    partial void OnSelectedFontChanged(object? value)
    {
        if (value is string name && name != _appearance.FontFamilyId)
        {
            _appearance.SetFontFamily(name);
        }
    }

    partial void OnSelectedScaleChanged(object? value)
    {
        if (value is double scale && scale != _appearance.FontScale)
        {
            _appearance.SetFontScale(scale);
        }
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        _appearance.ResetToDefaults();
    }

    private void OnAppearanceChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(AppearanceService.ThemeId):
                SelectedThemeId = _appearance.ThemeId;
                break;
            case nameof(AppearanceService.FontFamilyId):
                SelectedFont = _appearance.FontFamilyId;
                break;
            case nameof(AppearanceService.FontScale):
                SelectedScale = _appearance.FontScale;
                break;
        }
    }
}
