using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// The round tuning knob of <see cref="TuningDial"/>: its focusable control. A press steps to the next station and a
/// swipe on it steps either way; on Windows Left/Right step and Enter or Space plays. The knob turns 30° with each
/// step (at once with reduced motion). Original art; every colour comes from the palette.
/// </summary>
public class TuningKnob : ContentView
{
    private const double DegreesPerStep = 30;

    private readonly Grid _face;
    private readonly Button _button = new() { BackgroundColor = Colors.Transparent, BorderWidth = 0, Padding = 0 };
    private double _angle;

    public TuningKnob()
    {
        // A ridged face: an outer ring, the face, and a grip mark that shows the turn.
        var ring = new Ellipse { StrokeThickness = 2 };
        ring.SetDynamicResource(Shape.FillProperty, "DividerColor");
        ring.SetDynamicResource(Shape.StrokeProperty, "TabBarBackgroundColor");
        var face = new Ellipse { Margin = 5 };
        face.SetDynamicResource(Shape.FillProperty, "InputBackgroundColor");
        var mark = new BoxView { WidthRequest = 4, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, 8, 0, 0), CornerRadius = 2 };
        mark.SetDynamicResource(BoxView.ColorProperty, "FavoriteIconColor");
        SizeChanged += (_, _) => mark.HeightRequest = Math.Max(0, Height * 0.25);

        _face = new Grid { InputTransparent = true, Children = { ring, face, mark } };
        AutomationProperties.SetIsInAccessibleTree(_face, false);

        _button.Clicked += (_, _) => Stepped?.Invoke(this, 1);
        ButtonSwipe.Attach(_button, horizontal: true, step => Stepped?.Invoke(this, step));
        _button.HandlerChanged += (_, _) => AttachPlatformInput();

        Content = new Grid { Children = { _face, _button } };
    }

    /// <summary>
    /// +1 for the next station, -1 for the previous one.
    /// </summary>
    public event EventHandler<int>? Stepped;

    /// <summary>
    /// Enter or Space on Windows: play the focused station.
    /// </summary>
    public event EventHandler? Activated;

    /// <summary>
    /// The control screen readers and the keyboard reach.
    /// </summary>
    public Button Button => _button;

    /// <summary>
    /// Turns the knob by one step's angle per station moved.
    /// </summary>
    public void Turn(int steps)
    {
        _angle += steps * DegreesPerStep;
        _face.CancelAnimations();
        if (MotionSettings.IsReducedMotion)
        {
            _face.Rotation = _angle;
        }
        else
        {
            _ = _face.RotateToAsync(_angle, 250, Easing.CubicOut);
        }
    }

    // Only the Windows keys raise it; a method keeps the event "used" on every platform.
    private void RaiseActivated() => Activated?.Invoke(this, EventArgs.Empty);

    private void AttachPlatformInput()
    {
#if WINDOWS
        if (_button.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        // Before the button's own handling, so Enter and Space play instead of stepping.
        view.PreviewKeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case global::Windows.System.VirtualKey.Enter or global::Windows.System.VirtualKey.Space:
                    RaiseActivated();
                    e.Handled = true;
                    break;
                case global::Windows.System.VirtualKey.Right or global::Windows.System.VirtualKey.Left:
                    Stepped?.Invoke(this, e.Key == global::Windows.System.VirtualKey.Right ? 1 : -1);
                    e.Handled = true;
                    break;
            }
        };
#endif
    }
}
