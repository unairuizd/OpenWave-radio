using CommunityToolkit.Mvvm.ComponentModel;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>
/// One option of a settings choice (theme card, variant, font chip, text size, language, content type): its value,
/// its name in the current language and whether it is selected. Screen readers hear the name and, for the
/// selected option, that it is selected.
/// </summary>
public partial class ChoiceItem : ObservableObject
{
    private readonly Func<string> _name;

    public ChoiceItem(string value, Func<string> name)
    {
        Value = value;
        _name = name;
    }

    public string Value { get; }

    public string Name => _name();

    public string AccessibleName
    {
        get
        {
            var name = Description?.Invoke() ?? Name;
            return IsSelected ? Localizer.Instance.Format("Nav_Selected", name) : name;
        }
    }

    // What screen readers say instead of the name, when the name alone is not clear (the "+N" chip).
    public Func<string>? Description { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccessibleName))]
    public partial bool IsSelected { get; set; }

    // Font chips hidden behind "+N".
    [ObservableProperty]
    public partial bool IsShown { get; set; } = true;

    // The font a chip is written in, or the size of a text size sample.
    public string? FontFamily { get; init; }

    public double SampleSize { get; init; }

    // A theme card's colour sample.
    public IReadOnlyList<Color> Swatches { get; init; } = [];

    /// <summary>Re-reads the name after the language changed.</summary>
    public void RefreshName()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(AccessibleName));
    }
}
