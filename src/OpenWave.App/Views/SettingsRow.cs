using System.Windows.Input;
using OpenWave.App.Behaviors;

namespace OpenWave.App.Views;

/// <summary>
/// A full-width row of the settings pages: an optional icon, the title, a description or a value under it, and
/// <c>›</c>. An invisible button covers the row, so it is reached with the keyboard and announced once by screen
/// readers (title, then value or description).
/// </summary>
public class SettingsRow : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(SettingsRow), propertyChanged: OnChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingsRow), string.Empty, propertyChanged: OnChanged);

    public static readonly BindableProperty DetailProperty =
        BindableProperty.Create(nameof(Detail), typeof(string), typeof(SettingsRow), string.Empty, propertyChanged: OnChanged);

    // A value (Preferencias rows) is shown on the right, in the body size; a description goes under the title.
    public static readonly BindableProperty DetailIsValueProperty =
        BindableProperty.Create(nameof(DetailIsValue), typeof(bool), typeof(SettingsRow), false, propertyChanged: OnChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SettingsRow), propertyChanged: OnChanged);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SettingsRow), propertyChanged: OnChanged);

    private readonly Image _icon;
    private readonly Label _title;
    private readonly Label _description;
    private readonly Label _value;
    private readonly Button _button;

    public SettingsRow()
    {
        var resources = Application.Current!.Resources;

        _icon = new Image { WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
        _icon.SetDynamicResource(IconTint.ColorProperty, "AccentColor");

        _title = new Label { Style = (Style)resources["SettingsRowTitle"] };
        _description = new Label { Style = (Style)resources["SettingsRowDescription"] };
        _value = new Label
        {
            Style = (Style)resources["SettingsRowValue"],
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.End,
        };
        var chevron = new Label { Style = (Style)resources["SettingsChevron"], Text = "›" };

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { _title, _description } };

        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions = new ColumnDefinitionCollection(
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)),
        };
        grid.Add(_icon, 0, 0);
        grid.Add(texts, 1, 0);
        grid.Add(_value, 2, 0);
        grid.Add(chevron, 3, 0);
        foreach (var child in new View[] { _icon, _title, _description, _value, chevron })
        {
            AutomationProperties.SetIsInAccessibleTree(child, false);
        }

        var border = new Border { Style = (Style)resources["SettingsRow"], Content = grid };

        _button = new Button { Style = (Style)resources["RowButton"] };

        Content = new Grid { Children = { border, _button } };
        Update();
    }

    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public bool DetailIsValue
    {
        get => (bool)GetValue(DetailIsValueProperty);
        set => SetValue(DetailIsValueProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) => ((SettingsRow)bindable).Update();

    private void Update()
    {
        _icon.Source = Icon;
        _icon.IsVisible = Icon is not null;
        _title.Text = Title;
        _description.Text = DetailIsValue ? string.Empty : Detail;
        _description.IsVisible = !DetailIsValue && !string.IsNullOrEmpty(Detail);
        _value.Text = DetailIsValue ? Detail : string.Empty;
        _value.IsVisible = DetailIsValue;
        _button.Command = Command;
        _button.CommandParameter = CommandParameter;
        SemanticProperties.SetDescription(_button, string.IsNullOrEmpty(Detail) ? Title : $"{Title}, {Detail}");
        ToolTipProperties.SetText(_button, Title);
    }
}
