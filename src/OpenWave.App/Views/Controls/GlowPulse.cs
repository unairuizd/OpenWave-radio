namespace OpenWave.App.Views.Controls;

/// <summary>
/// A slow glow for a lit pad: while both <see cref="IsActive"/> (a station plays) and <see cref="IsCurrent"/> (this pad
/// holds it) are true, its content's opacity goes 1 → 0.7 → 1 every second. It is steady with reduced motion and while
/// its page is hidden. It never takes touches and adds nothing for screen readers.
/// </summary>
public class GlowPulse : PageAwareView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(GlowPulse), false, propertyChanged: (b, _, _) => ((GlowPulse)b).UpdateMotion());

    public static readonly BindableProperty IsCurrentProperty = BindableProperty.Create(
        nameof(IsCurrent), typeof(bool), typeof(GlowPulse), false, propertyChanged: (b, _, _) => ((GlowPulse)b).UpdateMotion());

    private const string AnimationName = "GlowPulse";

    public GlowPulse()
    {
        InputTransparent = true;
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public bool IsCurrent
    {
        get => (bool)GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateMotion();

    private void UpdateMotion()
    {
        this.AbortAnimation(AnimationName);
        if (!IsActive || !IsCurrent || !IsShown || MotionSettings.IsReducedMotion)
        {
            Opacity = 1;
            return;
        }

        var glow = new Animation
        {
            { 0, 0.5, new Animation(v => Opacity = v, 1, 0.7, Easing.SinInOut) },
            { 0.5, 1, new Animation(v => Opacity = v, 0.7, 1, Easing.SinInOut) },
        };
        glow.Commit(this, AnimationName, length: 1000, repeat: () => IsActive && IsCurrent && IsShown);
    }
}
