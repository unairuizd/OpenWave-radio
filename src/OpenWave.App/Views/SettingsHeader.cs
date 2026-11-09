using OpenWave.App.Behaviors;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views;

/// <summary>
/// The header of the settings pages, the same in every theme: the back arrow on the left, the page title as written
/// (not in upper case), centred, and the gear on the right. Colours, font and size come from the palette and Styles.xaml.
/// </summary>
public class SettingsHeader : ContentView
{
    private readonly ImageButton _back;
    private readonly ImageButton _gear;
    private readonly Label _title;

    public SettingsHeader()
    {
        var iconStyle = (Style)Application.Current!.Resources["IconButton"];

        _back = new ImageButton
        {
            Source = "ic_back.png",
            Command = AppNavigation.GoBackCommand,
            Style = iconStyle,
            Margin = new Thickness(4, 4, 0, 4),
        };
        _back.SetDynamicResource(IconTint.ColorProperty, "PrimaryTextColor");

        _title = new Label
        {
            Style = (Style)Application.Current.Resources["HeaderTitleLabel"],
            Margin = new Thickness(4, 0),
            LineBreakMode = LineBreakMode.WordWrap,
            MaxLines = 2,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level1);

        _gear = new ImageButton
        {
            Source = "ic_settings.png",
            Command = AppNavigation.OpenAjustesCommand,
            Style = iconStyle,
            Margin = new Thickness(0, 4, 4, 4),
        };
        _gear.SetDynamicResource(IconTint.ColorProperty, "PrimaryTextColor");

        var divider = new BoxView { HeightRequest = 1 };
        divider.SetDynamicResource(BoxView.ColorProperty, "DividerColor");

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)),
            RowDefinitions = new RowDefinitionCollection(
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)),
        };
        grid.SetDynamicResource(BackgroundColorProperty, "PageBackgroundColor");
        grid.Add(_back, 0, 0);
        grid.Add(_title, 1, 0);
        grid.Add(_gear, 2, 0);
        grid.Add(divider, 0, 1);
        Grid.SetColumnSpan(divider, 3);
        Content = grid;

        UpdateTexts();
        Loaded += (_, _) =>
        {
            if (FindPage() is { } page)
            {
                _title.SetBinding(Label.TextProperty, new Binding(nameof(Page.Title), source: page));
            }

            UpdateTexts();
            Localizer.Instance.LanguageChanged += OnLanguageChanged;
        };
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => UpdateTexts();

    private void UpdateTexts()
    {
        var localizer = Localizer.Instance;
        SemanticProperties.SetDescription(_back, localizer["Nav_Back"]);
        ToolTipProperties.SetText(_back, localizer["Nav_Back"]);
        SemanticProperties.SetDescription(_gear, localizer["Nav_Ajustes"]);
        ToolTipProperties.SetText(_gear, localizer["Nav_Ajustes"]);
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
