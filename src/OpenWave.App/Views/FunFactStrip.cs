using System.ComponentModel;
using OpenWave.App.Behaviors;
using OpenWave.App.Services;
using OpenWave.App.Services.Localization;
using OpenWave.App.Views.Controls;

namespace OpenWave.App.Views;

/// <summary>
/// The "Dato del día" strip shown by every theme's Inicio under the filters: a bulb, the caption, the first line of
/// today's fact and <c>›</c>. It reads the shared <see cref="FactsService"/>; logged out it invites to log in and
/// opens the login page, otherwise it opens Curiosidades. Drawn only with the semantic keys, so each theme's
/// colours, font and text size apply; width and height follow the station row limits.
/// </summary>
public class FunFactStrip : ContentView
{
    private readonly Label _caption;
    private readonly Label _text;
    private readonly Button _button;
    private FactsService? _facts;

    public FunFactStrip()
    {
        var resources = Application.Current!.Resources;

        var icon = new Image { Source = "ic_bulb.png", WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
        icon.SetDynamicResource(IconTint.ColorProperty, "AccentColor");

        _caption = new Label { FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
        _caption.SetDynamicResource(Label.FontFamilyProperty, "AppFontFamilyBold");
        _caption.SetDynamicResource(Label.FontSizeProperty, "FontSizeCaption");
        _caption.SetDynamicResource(Label.TextColorProperty, "AccentColor");

        _text = new Label { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
        _text.SetDynamicResource(Label.FontFamilyProperty, "AppFontFamily");
        _text.SetDynamicResource(Label.FontSizeProperty, "FontSizeBody");
        _text.SetDynamicResource(Label.TextColorProperty, "PrimaryTextColor");

        var chevron = new Label { Style = (Style)resources["SettingsChevron"], Text = "›" };

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { _caption, _text } };
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)),
        };
        grid.Add(icon, 0, 0);
        grid.Add(texts, 1, 0);
        grid.Add(chevron, 2, 0);
        foreach (var child in new View[] { icon, _caption, _text, chevron })
        {
            AutomationProperties.SetIsInAccessibleTree(child, false);
        }

        var border = new Border { Style = (Style)resources["StationCard"], Content = grid };
        border.SetDynamicResource(MinimumHeightRequestProperty, "StationRowMinHeight");
        border.SetDynamicResource(MaximumHeightRequestProperty, "StationRowMaxHeight");

        _button = new Button { Style = (Style)resources["RowButton"] };
        _button.Clicked += OnClicked;

        var limit = new SizeLimit();
        limit.SetDynamicResource(SizeLimit.MinWidthProperty, "StationRowMinWidth");
        limit.SetDynamicResource(SizeLimit.MaxWidthProperty, "StationRowMaxWidth");
        limit.Children.Add(new Grid { Children = { border, _button } });
        Content = limit;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _facts ??= IPlatformApplication.Current?.Services.GetService<FactsService>();
        if (_facts is not null)
        {
            _facts.PropertyChanged += OnFactsChanged;
            _ = _facts.LoadIfNeededAsync();
        }

        Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Update();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_facts is not null)
        {
            _facts.PropertyChanged -= OnFactsChanged;
        }

        Localizer.Instance.LanguageChanged -= OnLanguageChanged;
    }

    private void OnFactsChanged(object? sender, PropertyChangedEventArgs e) => Update();

    private void OnLanguageChanged(object? sender, EventArgs e) => Update();

    private async void OnClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(_facts?.IsAvailable == true ? "curiosidades" : "login");
    }

    private void Update()
    {
        var localizer = Localizer.Instance;
        _caption.Text = localizer["Facts_Caption"];
        _text.Text = _facts switch
        {
            null or { IsAvailable: false } => localizer["Facts_LogInHint"],
            { ErrorMessage: not null } => localizer["Facts_Offline"],
            { TodayFact: { } fact } => fact.Text,
            _ => localizer["Facts_Loading"],
        };
        SemanticProperties.SetDescription(_button, localizer.Format("Facts_Accessible", _text.Text));
        ToolTipProperties.SetText(_button, _text.Text);
    }
}
