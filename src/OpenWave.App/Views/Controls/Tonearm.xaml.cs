using System.Windows.Input;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// The tonearm: pressing it runs <see cref="Command"/> (play/stop). It only shows <see cref="IsLowered"/>: it swings
/// onto the record in about 600 ms with a short settle, or lifts off in about 500 ms; it jumps to its position when its
/// page is shown again or the system asks for reduced motion.
/// </summary>
public partial class Tonearm : PageAwareView
{
    public static readonly BindableProperty IsLoweredProperty = BindableProperty.Create(
        nameof(IsLowered), typeof(bool), typeof(Tonearm), false, propertyChanged: (b, _, _) => ((Tonearm)b).OnIsLoweredChanged());

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(Tonearm));

    private const double RaisedAngle = -6;
    private const double LoweredAngle = 15;
    private const double SettleAngle = 2;
    private const double TouchWidth = 56;

    public Tonearm()
    {
        // The descriptions follow the app language.
        Loaded += (_, _) => Localizer.Instance.LanguageChanged += OnLanguageChanged;
        Unloaded += (_, _) => Localizer.Instance.LanguageChanged -= OnLanguageChanged;

        InitializeComponent();
        SizeChanged += (_, _) => UpdateGeometry();
        UpdateDescription();
        Arm.Rotation = RaisedAngle;
    }

    /// <summary>
    /// True while a station is buffering or playing: the stylus rests on the record.
    /// </summary>
    public bool IsLowered
    {
        get => (bool)GetValue(IsLoweredProperty);
        set => SetValue(IsLoweredProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnShownChanged(bool shown)
    {
        Arm.CancelAnimations();
        Arm.Rotation = TargetAngle;
    }

    private double TargetAngle => IsLowered ? LoweredAngle : RaisedAngle;

    private void OnHitAreaClicked(object? sender, EventArgs e)
    {
        if (Command?.CanExecute(null) == true)
        {
            Command.Execute(null);
        }
    }

    private async void OnIsLoweredChanged()
    {
        UpdateDescription();
        Arm.CancelAnimations();

        if (!IsShown || MotionSettings.IsReducedMotion)
        {
            Arm.Rotation = TargetAngle;
            return;
        }

        if (!IsLowered)
        {
            await Arm.RotateToAsync(RaisedAngle, 500, Easing.CubicInOut);
            return;
        }

        // Lower, then settle on the record; RotateTo returns true when a newer state change cancelled it.
        if (await Arm.RotateToAsync(LoweredAngle, 600, Easing.CubicOut)
            || await Arm.RotateToAsync(LoweredAngle + SettleAngle, 90, Easing.SinOut))
        {
            return;
        }

        await Arm.RotateToAsync(LoweredAngle, 90, Easing.SinIn);
    }

    private void UpdateDescription()
    {
        SemanticProperties.SetDescription(HitArea, Localizer.Instance[IsLowered ? "Tonearm_Stop" : "Tonearm_Play"]);
    }

    // The geometry follows the record drawn in the same cell: a circle as large as the cell allows.
    private void UpdateGeometry()
    {
        var radius = Math.Max(0, Math.Min(Width, Height)) * VinylDisc.SizeInCell / 2;
        if (radius <= 0)
        {
            return;
        }

        var centerX = Width / 2;
        var centerY = Height / 2;
        var pivotX = centerX + radius * 0.92;
        var pivotY = centerY - radius * 0.92;

        var armLength = radius * 1.25;
        var aboveThePivot = radius * 0.2;
        var rod = Math.Max(3, radius * 0.025);

        Arm.WidthRequest = TouchWidth;
        Arm.HeightRequest = aboveThePivot + armLength;
        Arm.TranslationX = pivotX - TouchWidth / 2;
        Arm.TranslationY = pivotY - aboveThePivot;
        Arm.AnchorX = 0.5;
        Arm.AnchorY = aboveThePivot / (aboveThePivot + armLength);

        Rod.WidthRequest = rod;
        Counterweight.WidthRequest = rod * 4;
        Counterweight.HeightRequest = radius * 0.14;
        Headshell.WidthRequest = rod * 3;
        Headshell.HeightRequest = radius * 0.12;

        var pivot = radius * 0.22;
        PivotBase.WidthRequest = PivotBase.HeightRequest = pivot;
        PivotBase.TranslationX = pivotX - pivot / 2;
        PivotBase.TranslationY = pivotY - pivot / 2;
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => UpdateDescription();
}
