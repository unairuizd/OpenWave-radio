using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// The window of a cassette player: the tape shell, its silver label with the station logo and name, and the two reels.
/// While <see cref="IsSpinning"/> the reels turn at half a turn per second, speeding up over about 0.8 s and slowing
/// down over about 1 s from their current angle (one <see cref="SpinDriver"/> for both). They stop while the page is
/// hidden, are already turning when it is shown during playback, and stay still with reduced motion. Decorative: the
/// station name is read elsewhere, so the window is hidden from screen readers. Original art; colours from the palette.
/// </summary>
public class CassetteReels : PageAwareView
{
    public static readonly BindableProperty IsSpinningProperty = BindableProperty.Create(
        nameof(IsSpinning), typeof(bool), typeof(CassetteReels), false, propertyChanged: (b, _, _) => ((CassetteReels)b).UpdateMotion(false));

    public static readonly BindableProperty LogoSourceProperty = BindableProperty.Create(
        nameof(LogoSource), typeof(string), typeof(CassetteReels), null, propertyChanged: (b, _, _) => ((CassetteReels)b).UpdateLabel());

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CassetteReels), null, propertyChanged: (b, _, _) => ((CassetteReels)b).UpdateLabel());

    private const double TurnSpeed = 180; // degrees per second
    private const double SpinUpSeconds = 0.8;
    private const double SpinDownSeconds = 1.0;

    private readonly SpinDriver _spin;
    private readonly Grid _leftReel;
    private readonly Grid _rightReel;
    private readonly Image _logo = new() { Aspect = Aspect.AspectFit, WidthRequest = 28, HeightRequest = 28 };
    private readonly Image _fallback = new() { Aspect = Aspect.AspectFit, WidthRequest = 28, HeightRequest = 28, Source = "station_placeholder.png" };
    private readonly Label _title = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };

    public CassetteReels()
    {
        // The shell: a dark plastic body with a lighter edge.
        var shell = new Border { StrokeThickness = 3, Padding = new Thickness(14, 10) };
        shell.StrokeShape = new RoundRectangle { CornerRadius = 10 };
        shell.SetDynamicResource(BackgroundColorProperty, "InputBackgroundColor");
        shell.SetDynamicResource(Border.StrokeProperty, "DividerColor");

        // The silver label with the logo and the hand-written name (dark text on silver).
        _title.SetDynamicResource(StyleProperty, "StationNameLabel");
        _title.SetDynamicResource(Label.TextColorProperty, "AccentTextColor");
        var label = new Border
        {
            Padding = new Thickness(8, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            Content = new Grid
            {
                ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)],
                ColumnSpacing = 8,
                Children = { new Grid { Children = { _fallback, _logo } }, _title },
            },
        };
        label.SetDynamicResource(BackgroundColorProperty, "TabBarUnselectedColor");
        Grid.SetColumn(_title, 1);

        // The tape window between the reels, and the reels.
        _leftReel = CreateReel();
        _rightReel = CreateReel();
        var tape = new Border { StrokeThickness = 2, HeightRequest = 18, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(30, 0) };
        tape.StrokeShape = new RoundRectangle { CornerRadius = 9 };
        tape.SetDynamicResource(BackgroundColorProperty, "PageBackgroundColor");
        tape.SetDynamicResource(Border.StrokeProperty, "DividerColor");
        var window = new Grid { HeightRequest = 64, Children = { tape, _leftReel, _rightReel } };
        _leftReel.HorizontalOptions = LayoutOptions.Start;
        _rightReel.HorizontalOptions = LayoutOptions.End;

        shell.Content = new VerticalStackLayout { Spacing = 10, Children = { label, window } };
        Content = shell;

        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        _spin = new SpinDriver(Dispatcher, TurnSpeed, SpinUpSeconds, SpinDownSeconds, degrees =>
        {
            _leftReel.Rotation = (_leftReel.Rotation + degrees) % 360;
            _rightReel.Rotation = (_rightReel.Rotation + degrees) % 360;
        });
        UpdateLabel();
    }

    public bool IsSpinning
    {
        get => (bool)GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }

    /// <summary>
    /// The station logo on the label; the fallback image is shown when it is missing.
    /// </summary>
    public string? LogoSource
    {
        get => (string?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    /// <summary>
    /// The name written on the label; blank when no station is selected.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateMotion(shown);

    // A reel: the hub with six teeth, inside a ring of wound tape.
    private static Grid CreateReel()
    {
        var tape = new Ellipse { StrokeThickness = 6 };
        tape.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        var hub = new Ellipse { Margin = 14 };
        hub.SetDynamicResource(Shape.FillProperty, "TabBarUnselectedColor");
        var reel = new Grid { WidthRequest = 60, HeightRequest = 60, Children = { tape, hub } };
        for (var i = 0; i < 3; i++)
        {
            var tooth = new BoxView { WidthRequest = 4, HeightRequest = 30, Rotation = i * 60, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            tooth.SetDynamicResource(BoxView.ColorProperty, "InputBackgroundColor");
            reel.Children.Add(tooth);
        }

        return reel;
    }

    private void UpdateLabel()
    {
        _title.Text = Title;
        _logo.Source = LogoSource;
        _logo.IsVisible = !string.IsNullOrEmpty(LogoSource);
        _fallback.IsVisible = !string.IsNullOrEmpty(Title);
    }

    private void UpdateMotion(bool jumpToState)
    {
        var moving = IsSpinning && IsShown && !MotionSettings.IsReducedMotion;
        if (jumpToState || !IsShown)
        {
            _spin.SnapToState(moving);
        }
        else
        {
            _spin.SetMoving(moving);
        }
    }
}
