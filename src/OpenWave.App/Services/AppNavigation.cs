using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using OpenWave.App.Views;

namespace OpenWave.App.Services;

/// <summary>
/// The section a page belongs to; the bottom bar marks it. Settings pages belong to none.
/// </summary>
public enum AppSection
{
    None,
    Inicio,
    Reproductor,
    Favoritos,
}

/// <summary>
/// Navigation used by the shared header and bottom bar of every theme layout. It only moves between pages,
/// so the layouts can bind to it directly and no view model needs to know about the chrome.
/// </summary>
public static class AppNavigation
{
    /// <summary>
    /// Set on each themed page (for example <c>services:AppNavigation.Section="Inicio"</c>).
    /// </summary>
    public static readonly BindableProperty SectionProperty = BindableProperty.CreateAttached(
        "Section", typeof(AppSection), typeof(AppNavigation), AppSection.None);

    public static AppSection GetSection(BindableObject page) => (AppSection)page.GetValue(SectionProperty);

    public static void SetSection(BindableObject page, AppSection value) => page.SetValue(SectionProperty, value);

    /// <summary>
    /// The header's gear: opens Ajustes, returns to it from its sub-pages, and does nothing on Ajustes itself.
    /// </summary>
    public static ICommand OpenAjustesCommand { get; } = new AsyncRelayCommand(OpenAjustesAsync);

    /// <summary>
    /// A bottom bar button; the parameter is the section's route (<c>inicio</c>, <c>reproductor</c> or <c>favoritos</c>).
    /// </summary>
    public static ICommand GoToSectionCommand { get; } = new AsyncRelayCommand<string>(GoToSectionAsync);

    private static Task OpenAjustesAsync()
    {
        var shell = Shell.Current;
        var stack = shell.Navigation.NavigationStack;
        if (shell.CurrentPage is AjustesPage)
        {
            return Task.CompletedTask;
        }

        // Personalización or Preferencias is open on top of Ajustes: go back to it instead of pushing a second one.
        var ajustesIndex = stack.ToList().FindIndex(p => p is AjustesPage);
        if (ajustesIndex >= 0)
        {
            var levels = stack.Count - 1 - ajustesIndex;
            return shell.GoToAsync(string.Join("/", Enumerable.Repeat("..", levels)));
        }

        return shell.GoToAsync("ajustes");
    }

    private static Task GoToSectionAsync(string? route)
    {
        var shell = Shell.Current;
        if (route is null || !Enum.TryParse<AppSection>(route, ignoreCase: true, out var section))
        {
            return Task.CompletedTask;
        }

        // Already on that section's own page: nothing to do (playback in Reproductor goes on).
        if (shell.Navigation.NavigationStack.Count <= 1 && shell.CurrentPage is { } page && GetSection(page) == section)
        {
            return Task.CompletedTask;
        }

        // An absolute route also closes any settings pages pushed on top.
        return shell.GoToAsync("//" + route);
    }
}
