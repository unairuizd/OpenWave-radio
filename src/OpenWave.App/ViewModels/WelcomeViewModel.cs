using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.ViewModels;

/// <summary>How the user left the welcome pop-up. A dismissed pop-up (back button, tap outside) gives no result.</summary>
public enum WelcomeChoice
{
    Continue,
    Login,
    Register,
}

/// <summary>
/// The welcome pop-up: the language chips (applied and stored at once, like in Personalización) and the three
/// ways out. The view closes the pop-up with the chosen <see cref="WelcomeChoice"/>.
/// </summary>
public partial class WelcomeViewModel : ObservableObject
{
    private readonly AppearanceService _appearance;

    public WelcomeViewModel(AppearanceService appearance)
    {
        _appearance = appearance;
        Languages = LanguageCatalog.All
            .Select(l => new ChoiceItem(l.Code, () => l.DisplayName))
            .ToList();
        UpdateSelection();
        Localizer.Instance.LanguageChanged += (_, _) => UpdateSelection();
    }

    public IReadOnlyList<ChoiceItem> Languages { get; }

    /// <summary>Raised with the user's choice; the pop-up closes itself with it.</summary>
    public event EventHandler<WelcomeChoice>? Chosen;

    [RelayCommand]
    private void SelectLanguage(ChoiceItem item) => _appearance.SetLanguage(item.Value);

    [RelayCommand]
    private void Login() => Chosen?.Invoke(this, WelcomeChoice.Login);

    [RelayCommand]
    private void Register() => Chosen?.Invoke(this, WelcomeChoice.Register);

    [RelayCommand]
    private void Continue() => Chosen?.Invoke(this, WelcomeChoice.Continue);

    private void UpdateSelection()
    {
        foreach (var language in Languages)
        {
            language.IsSelected = language.Value == _appearance.LanguageCode;
            language.RefreshName();
        }
    }
}
