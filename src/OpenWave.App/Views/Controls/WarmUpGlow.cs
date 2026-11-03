namespace OpenWave.App.Views.Controls;

/// <summary>
/// The valve radio's dial lamp: its content glows in over about 1.5 s when <see cref="IsActive"/> becomes true (the
/// valves warming up) and dims back in about 0.6 s on stop. A page shown while a station plays, and reduced motion,
/// show the final state at once. Decorative: hidden from screen readers and lets touches through.
/// </summary>
public class WarmUpGlow : PageAwareView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(WarmUpGlow), false, propertyChanged: (b, _, _) => ((WarmUpGlow)b).UpdateState(false));

    private const double OffOpacity = 0.3;

    public WarmUpGlow()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        Opacity = OffOpacity;
    }

    /// <summary>
    /// True while a station is buffering or playing.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateState(true);

    private void UpdateState(bool jumpToState)
    {
        var target = IsActive ? 1 : OffOpacity;
        this.CancelAnimations();
        if (jumpToState || !IsShown || MotionSettings.IsReducedMotion)
        {
            Opacity = target;
            return;
        }

        _ = this.FadeToAsync(target, IsActive ? 1500u : 600u, Easing.CubicInOut);
    }
}
