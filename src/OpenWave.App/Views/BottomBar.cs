using OpenWave.App.Services;

namespace OpenWave.App.Views;

/// <summary>
/// Host of the theme's bottom bar (its look is the layout's BottomBarTemplate). It exposes the section of the page
/// it is on, read from <see cref="AppNavigation.SectionProperty"/>, so the template can mark that section's button.
/// </summary>
public class BottomBar : ContentView
{
    public static readonly BindableProperty SectionProperty = BindableProperty.Create(
        nameof(Section), typeof(AppSection), typeof(BottomBar), AppSection.None, propertyChanged: OnSectionChanged);

    public BottomBar()
    {
        // The page is known once the bar is shown. Reading it before the platform views are created means each icon
        // is tinted once, with its final colour.
        HandlerChanging += (_, _) => UpdateSection();
        Loaded += (_, _) => UpdateSection();
    }

    private void UpdateSection()
    {
        if (FindPage() is { } page)
        {
            Section = AppNavigation.GetSection(page);
        }
    }

    public AppSection Section
    {
        get => (AppSection)GetValue(SectionProperty);
        private set => SetValue(SectionProperty, value);
    }

    // One flag per button, so the template's triggers can set both the marked and the unmarked look.
    public bool IsInicio => Section == AppSection.Inicio;

    public bool IsReproductor => Section == AppSection.Reproductor;

    public bool IsFavoritos => Section == AppSection.Favoritos;

    private static void OnSectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var bar = (BottomBar)bindable;
        bar.OnPropertyChanged(nameof(IsInicio));
        bar.OnPropertyChanged(nameof(IsReproductor));
        bar.OnPropertyChanged(nameof(IsFavoritos));
    }

    private Page? FindPage()
    {
        Element? element = Parent;
        while (element is not null and not Page)
        {
            element = element.Parent;
        }

        return element as Page;
    }
}
