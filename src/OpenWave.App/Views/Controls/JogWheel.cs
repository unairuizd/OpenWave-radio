using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// The jog wheel of the DJ deck: a cyan platter ring with ticks around a dark platter, and the station logo in the
/// centre. While <see cref="IsSpinning"/> the platter turns like the Tocadiscos record (33⅓ rpm, speeding up over
/// about 1.2 s and slowing down over about 1.8 s from its current angle, with one <see cref="SpinDriver"/>). It stops
/// while the page is hidden, is already turning when the page is shown during playback, and stays still with reduced
/// motion. Decorative: hidden from screen readers. Original art; colours from the palette.
/// </summary>
public class JogWheel : PageAwareView
{
    public static readonly BindableProperty IsSpinningProperty = BindableProperty.Create(
        nameof(IsSpinning), typeof(bool), typeof(JogWheel), false, propertyChanged: (b, _, _) => ((JogWheel)b).UpdateMotion(false));

    public static readonly BindableProperty LogoSourceProperty = BindableProperty.Create(
        nameof(LogoSource), typeof(string), typeof(JogWheel), null, propertyChanged: (b, _, _) => ((JogWheel)b).UpdateCentre());

    public static readonly BindableProperty IsBlankProperty = BindableProperty.Create(
        nameof(IsBlank), typeof(bool), typeof(JogWheel), false, propertyChanged: (b, _, _) => ((JogWheel)b).UpdateCentre());

    private const double TurnSpeed = 360.0 / (60.0 / (100.0 / 3.0)); // degrees per second at 33⅓ rpm
    private const double SpinUpSeconds = 1.2;
    private const double SpinDownSeconds = 1.8;
    private const int Ticks = 12;

    private readonly SpinDriver _spin;
    private readonly Grid _platter = new();
    private readonly Ellipse _centre = new();
    private readonly Image _logo = new() { Aspect = Aspect.AspectFill };
    private readonly Image _fallback = new() { Aspect = Aspect.AspectFill, Source = "station_placeholder.png" };

    public JogWheel()
    {
        var ring = new Ellipse { StrokeThickness = 6 };
        ring.SetDynamicResource(Shape.StrokeProperty, "AccentColor");
        ring.SetDynamicResource(Shape.FillProperty, "InputBackgroundColor");
        var groove = new Ellipse { Margin = 16, StrokeThickness = 2 };
        groove.SetDynamicResource(Shape.StrokeProperty, "DividerColor");

        // The ticks turn with the platter, so the rotation is visible.
        _platter.Children.Add(groove);
        for (var i = 0; i < Ticks; i++)
        {
            var tick = new BoxView { WidthRequest = 3, VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, 8, 0, 0), AnchorY = 0 };
            tick.SetDynamicResource(BoxView.ColorProperty, "DividerColor");
            _platter.Children.Add(tick);
        }

        _centre.SetDynamicResource(Shape.FillProperty, "SurfaceColor");
        _centre.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        _centre.StrokeThickness = 2;
        var centre = new Grid { Children = { _centre, _fallback, _logo }, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        _platter.Children.Add(centre);

        SizeChanged += (_, _) =>
        {
            var size = Math.Max(0, Math.Min(Width, Height));
            var radius = size / 2;
            for (var i = 1; i <= Ticks; i++)
            {
                var tick = (BoxView)_platter.Children[i];
                // Rotate each tick about the platter's centre: its top is 8 below the top edge.
                tick.HeightRequest = size * 0.08;
                tick.AnchorY = (radius - 8) / Math.Max(1, tick.HeightRequest);
                tick.Rotation = i * 360.0 / Ticks;
            }

            var label = size * 0.38;
            centre.WidthRequest = centre.HeightRequest = label;
            var logo = label * 0.8;
            foreach (var image in new[] { _logo, _fallback })
            {
                image.WidthRequest = image.HeightRequest = logo;
                image.Clip = new EllipseGeometry(new Point(logo / 2, logo / 2), logo / 2, logo / 2);
            }
        };

        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        Content = new Grid { Children = { ring, _platter } };
        _spin = new SpinDriver(Dispatcher, TurnSpeed, SpinUpSeconds, SpinDownSeconds, degrees => _platter.Rotation = (_platter.Rotation + degrees) % 360);
        UpdateCentre();
    }

    public bool IsSpinning
    {
        get => (bool)GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }

    /// <summary>
    /// The station logo in the centre; the fallback image is shown when it is missing.
    /// </summary>
    public string? LogoSource
    {
        get => (string?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    /// <summary>
    /// A blank centre (no station selected).
    /// </summary>
    public bool IsBlank
    {
        get => (bool)GetValue(IsBlankProperty);
        set => SetValue(IsBlankProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateMotion(shown);

    private void UpdateCentre()
    {
        _logo.Source = IsBlank ? null : LogoSource;
        _logo.IsVisible = !IsBlank && !string.IsNullOrEmpty(LogoSource);
        _fallback.IsVisible = !IsBlank;
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
