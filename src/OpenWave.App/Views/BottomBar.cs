using OpenWave.App.Behaviors;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views;

/// <summary>
/// The bottom bar, the same in every theme: three icons (house, radio, star) for Inicio, Reproductor and Favoritos,
/// coloured by the palette (TabBarBackgroundColor, TabBarSelectedColor, TabBarUnselectedColor). The section of the
/// page it is on, read from <see cref="AppNavigation.SectionProperty"/>, is marked with the selected colour and a
/// short line under its icon. Each page decides whether it shows the bar.
/// </summary>
public class BottomBar : ContentView
{
    public static readonly BindableProperty SectionProperty = BindableProperty.Create(
        nameof(Section), typeof(AppSection), typeof(BottomBar), AppSection.None, propertyChanged: OnSectionChanged);

    private readonly Item[] _items;

    public BottomBar()
    {
        _items =
        [
            new(AppSection.Inicio, "inicio", "ic_home.png", "Nav_Inicio"),
            new(AppSection.Reproductor, "reproductor", "ic_player.png", "Nav_Reproductor"),
            new(AppSection.Favoritos, "favoritos", "ic_favorites.png", "Nav_Favoritos"),
        ];

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)),
            RowDefinitions = new RowDefinitionCollection(
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)),
        };
        grid.SetDynamicResource(BackgroundColorProperty, "TabBarBackgroundColor");

        var divider = new BoxView { HeightRequest = 1 };
        divider.SetDynamicResource(BoxView.ColorProperty, "DividerColor");
        grid.Add(divider, 0, 0);
        Grid.SetColumnSpan(divider, 3);

        for (var column = 0; column < _items.Length; column++)
        {
            var item = _items[column];
            grid.Add(item.Button, column, 1);
            grid.Add(item.Marker, column, 2);
        }

        Content = grid;
        UpdateItems();

        // The page is known once the bar is shown. Reading it before the platform views are created means each icon
        // is tinted once, with its final colour.
        HandlerChanging += (_, _) => UpdateSection();
        Loaded += (_, _) =>
        {
            UpdateSection();
            UpdateTexts();
            Localizer.Instance.LanguageChanged += OnLanguageChanged;
        };
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;
    }

    public AppSection Section
    {
        get => (AppSection)GetValue(SectionProperty);
        private set => SetValue(SectionProperty, value);
    }

    private static void OnSectionChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((BottomBar)bindable).UpdateItems();

    private void OnLanguageChanged(object? sender, EventArgs e) => UpdateTexts();

    private void UpdateSection()
    {
        if (FindPage() is { } page)
        {
            Section = AppNavigation.GetSection(page);
        }
    }

    private void UpdateItems()
    {
        foreach (var item in _items)
        {
            var marked = item.Section == Section;
            item.Button.SetDynamicResource(IconTint.ColorProperty, marked ? "TabBarSelectedColor" : "TabBarUnselectedColor");
            item.Marker.Opacity = marked ? 1 : 0;
        }

        UpdateTexts();
    }

    // Screen readers say the section's name, and "selected" for the marked one; Windows shows the name as a tooltip.
    private void UpdateTexts()
    {
        var localizer = Localizer.Instance;
        foreach (var item in _items)
        {
            var name = localizer[item.NameKey];
            SemanticProperties.SetDescription(
                item.Button,
                item.Section == Section ? localizer.Format("Nav_Selected", name) : name);
            ToolTipProperties.SetText(item.Button, name);
        }
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

    private sealed class Item
    {
        public Item(AppSection section, string route, string icon, string nameKey)
        {
            Section = section;
            NameKey = nameKey;
            Button = new ImageButton
            {
                Source = icon,
                Command = AppNavigation.GoToSectionCommand,
                CommandParameter = route,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 4, 0, 0),
                Style = (Style)Application.Current!.Resources["IconButton"],
            };
            Marker = new BoxView
            {
                HeightRequest = 3,
                WidthRequest = 24,
                CornerRadius = 1.5,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 6),
                InputTransparent = true,
            };
            Marker.SetDynamicResource(BoxView.ColorProperty, "TabBarSelectedColor");
            AutomationProperties.SetIsInAccessibleTree(Marker, false);
        }

        public AppSection Section { get; }

        public string NameKey { get; }

        public ImageButton Button { get; }

        public BoxView Marker { get; }
    }
}
