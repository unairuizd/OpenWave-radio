using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// A round loudspeaker: a ridged surround, the dark cone and the dust cap. While <see cref="IsActive"/> the cone
/// pulses gently (scale 1 → 1.04 and back, 450 ms each way, about 133 beats per minute); every speaker uses the same
/// timing, so two speakers started together move in step. On stop it settles back to 1. It is still with reduced
/// motion and while its page is hidden. Decorative: hidden from screen readers and never takes touches. Original art;
/// colours from the palette.
/// </summary>
public class SpeakerCone : PageAwareView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(SpeakerCone), false, propertyChanged: (b, _, _) => ((SpeakerCone)b).UpdateMotion());

    private const string AnimationName = "SpeakerPulse";
    private const uint HalfBeat = 450;

    private readonly Grid _cone;

    public SpeakerCone()
    {
        var surround = new Ellipse { StrokeThickness = 4 };
        surround.SetDynamicResource(Shape.FillProperty, "SurfaceColor");
        surround.SetDynamicResource(Shape.StrokeProperty, "TabBarUnselectedColor");
        var ring = new Ellipse { Margin = 8, StrokeThickness = 2 };
        ring.SetDynamicResource(Shape.StrokeProperty, "DividerColor");

        var body = new Ellipse { Margin = 14 };
        body.SetDynamicResource(Shape.FillProperty, "InputBackgroundColor");
        var ridge = new Ellipse { Margin = 26, StrokeThickness = 1 };
        ridge.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        var cap = new Ellipse { WidthRequest = 0, HeightRequest = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        cap.SetDynamicResource(Shape.FillProperty, "SurfaceColor");
        cap.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        _cone = new Grid { Children = { body, ridge, cap } };

        SizeChanged += (_, _) =>
        {
            var size = Math.Min(Width, Height);
            cap.WidthRequest = cap.HeightRequest = Math.Max(0, size * 0.28);
        };

        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        Content = new Grid { Children = { surround, ring, _cone } };
    }

    /// <summary>
    /// True while a station is buffering or playing.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateMotion();

    private void UpdateMotion()
    {
        this.AbortAnimation(AnimationName);
        if (!IsActive || !IsShown || MotionSettings.IsReducedMotion)
        {
            _cone.CancelAnimations();
            _ = _cone.ScaleToAsync(1, IsShown && !MotionSettings.IsReducedMotion ? 200u : 0u);
            return;
        }

        var pulse = new Animation
        {
            { 0, 0.5, new Animation(v => _cone.Scale = v, 1, 1.04, Easing.SinInOut) },
            { 0.5, 1, new Animation(v => _cone.Scale = v, 1.04, 1, Easing.SinInOut) },
        };
        pulse.Commit(this, AnimationName, length: HalfBeat * 2, repeat: () => IsActive && IsShown);
    }
}
