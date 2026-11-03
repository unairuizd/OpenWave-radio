using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// A vinyl record that turns at 33⅓ rpm while <see cref="IsSpinning"/> is true. It only shows that state: it speeds
/// up over about 1.2 s, slows down with inertia over about 1.8 s, always from its current angle, stops turning while
/// its page is hidden, and stays still when the system asks for reduced motion.
/// </summary>
public partial class VinylDisc : PageAwareView
{
    public static readonly BindableProperty LogoSourceProperty = BindableProperty.Create(
        nameof(LogoSource), typeof(string), typeof(VinylDisc), null, propertyChanged: (b, _, _) => ((VinylDisc)b).UpdateLabel());

    public static readonly BindableProperty IsBlankProperty = BindableProperty.Create(
        nameof(IsBlank), typeof(bool), typeof(VinylDisc), false, propertyChanged: (b, _, _) => ((VinylDisc)b).UpdateLabel());

    public static readonly BindableProperty IsSpinningProperty = BindableProperty.Create(
        nameof(IsSpinning), typeof(bool), typeof(VinylDisc), false, propertyChanged: (b, _, _) => ((VinylDisc)b).UpdateMotion(false));

    /// <summary>
    /// Share of the cell the record takes, leaving room for the tonearm's pivot.
    /// </summary>
    public const double SizeInCell = 0.9;

    private const double TurnSpeed = 360.0 / (60.0 / (100.0 / 3.0)); // degrees per second at 33⅓ rpm
    private const double SpinUpSeconds = 1.2;
    private const double SpinDownSeconds = 1.8;
    private const double LabelSize = 0.38;
    private const double LogoSize = 0.30;

    private readonly SpinDriver _spin;

    public VinylDisc()
    {
        InitializeComponent();
        _spin = new SpinDriver(Dispatcher, TurnSpeed, SpinUpSeconds, SpinDownSeconds, degrees => Rotor.Rotation = (Rotor.Rotation + degrees) % 360);
        SizeChanged += (_, _) => UpdateSize();
        UpdateLabel();
    }

    /// <summary>
    /// The station logo shown on the label; the fallback image is shown when it is missing or cannot be loaded.
    /// </summary>
    public string? LogoSource
    {
        get => (string?)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    /// <summary>
    /// A record with a plain label (no station selected).
    /// </summary>
    public bool IsBlank
    {
        get => (bool)GetValue(IsBlankProperty);
        set => SetValue(IsBlankProperty, value);
    }

    public bool IsSpinning
    {
        get => (bool)GetValue(IsSpinningProperty);
        set => SetValue(IsSpinningProperty, value);
    }

    protected override void OnShownChanged(bool shown)
    {
        // Back on screen: show the current state at once (full speed or still), with no spin-up.
        UpdateMotion(shown);
    }

    // The record is a circle as large as the space allows, so it is never clipped and does not depend on text size.
    private void UpdateSize()
    {
        var diameter = Math.Max(0, Math.Min(Width, Height)) * SizeInCell;
        Disc.WidthRequest = Disc.HeightRequest = diameter;

        var logo = diameter * LogoSize;
        foreach (var image in new[] { Logo, Fallback })
        {
            image.WidthRequest = image.HeightRequest = logo;
            image.Clip = new EllipseGeometry(new Point(logo / 2, logo / 2), logo / 2, logo / 2);
        }

        var mark = diameter * 0.025;
        LabelMark.WidthRequest = LabelMark.HeightRequest = mark;
        LabelMark.Margin = new Thickness(0, diameter * (1 - LabelSize) / 2 + mark, 0, 0);

        Spindle.WidthRequest = Spindle.HeightRequest = diameter * 0.035;
    }

    private void UpdateLabel()
    {
        Logo.Source = IsBlank ? null : LogoSource;
        Logo.IsVisible = !IsBlank && !string.IsNullOrEmpty(LogoSource);
        Fallback.IsVisible = !IsBlank;
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
