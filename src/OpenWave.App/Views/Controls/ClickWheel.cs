using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// A round click wheel: a ring with four real buttons at its edge (MENÚ at the top, ◀◀ on the left, ▶▶ on the right,
/// play/pause at the bottom) and a centre button. Sliding a finger around the ring runs <see cref="ScrollStepCommand"/>
/// with +1 (clockwise) or -1 about every 24°, while <see cref="IsScrollEnabled"/>. The buttons give a short 80 ms press
/// feedback (none with reduced motion). On Windows, Escape runs MENÚ, Space play/pause and Left/Right ◀◀/▶▶ anywhere
/// on the page except in a text box; Up/Down, Enter and the mouse wheel belong to the list. Its size never depends on
/// the text size. Original art; colours from the palette.
/// </summary>
public class ClickWheel : ContentView
{
    public static readonly BindableProperty ScrollStepCommandProperty = BindableProperty.Create(nameof(ScrollStepCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty CenterCommandProperty = BindableProperty.Create(nameof(CenterCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty MenuCommandProperty = BindableProperty.Create(nameof(MenuCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty MenuCommandParameterProperty = BindableProperty.Create(nameof(MenuCommandParameter), typeof(object), typeof(ClickWheel));
    public static readonly BindableProperty IsMenuEnabledProperty = BindableProperty.Create(nameof(IsMenuEnabled), typeof(bool), typeof(ClickWheel), true, propertyChanged: (b, _, n) => ((ClickWheel)b)._menu.IsEnabled = (bool)n);
    public static readonly BindableProperty PreviousCommandProperty = BindableProperty.Create(nameof(PreviousCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty NextCommandProperty = BindableProperty.Create(nameof(NextCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty PlayPauseCommandProperty = BindableProperty.Create(nameof(PlayPauseCommand), typeof(ICommand), typeof(ClickWheel));
    public static readonly BindableProperty IsScrollEnabledProperty = BindableProperty.Create(nameof(IsScrollEnabled), typeof(bool), typeof(ClickWheel), true, propertyChanged: (b, _, _) => ((ClickWheel)b).UpdateRing());

    public static readonly BindableProperty PreviousDescriptionProperty = BindableProperty.Create(nameof(PreviousDescription), typeof(string), typeof(ClickWheel), "Anterior", propertyChanged: (b, _, n) => SemanticProperties.SetDescription(((ClickWheel)b)._previous, (string?)n));
    public static readonly BindableProperty NextDescriptionProperty = BindableProperty.Create(nameof(NextDescription), typeof(string), typeof(ClickWheel), "Siguiente", propertyChanged: (b, _, n) => SemanticProperties.SetDescription(((ClickWheel)b)._next, (string?)n));
    public static readonly BindableProperty PlayPauseDescriptionProperty = BindableProperty.Create(nameof(PlayPauseDescription), typeof(string), typeof(ClickWheel), "Reproducir o pausar", propertyChanged: (b, _, n) => SemanticProperties.SetDescription(((ClickWheel)b)._playPause, (string?)n));
    public static readonly BindableProperty CenterDescriptionProperty = BindableProperty.Create(nameof(CenterDescription), typeof(string), typeof(ClickWheel), "Seleccionar", propertyChanged: (b, _, n) => SemanticProperties.SetDescription(((ClickWheel)b)._center, (string?)n));
    public static readonly BindableProperty CenterIconProperty = BindableProperty.Create(nameof(CenterIcon), typeof(ImageSource), typeof(ClickWheel), null, propertyChanged: (b, _, n) => ((ClickWheel)b)._center.Source = (ImageSource?)n);

    private const double DegreesPerStep = 24;
    private const double Size = 220;
    private const double CenterSize = 84;

    private readonly Grid _ring = new() { WidthRequest = Size, HeightRequest = Size };
    private readonly Button _menu;
    private readonly Button _previous;
    private readonly Button _next;
    private readonly Button _playPause;
    private readonly ImageButton _center = new() { WidthRequest = CenterSize, HeightRequest = CenterSize, CornerRadius = (int)(CenterSize / 2), Padding = 24 };
    private double? _lastAngle;
    private double _turned;

    public ClickWheel()
    {
        var face = new Ellipse { StrokeThickness = 2 };
        face.SetDynamicResource(Shape.FillProperty, "InputBackgroundColor");
        face.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        _ring.Children.Add(face);

        _menu = CreateEdgeButton("MENÚ", LayoutOptions.Center, LayoutOptions.Start, () => Run(MenuCommand, MenuCommandParameter));
        SemanticProperties.SetDescription(_menu, "Menú");
        _previous = CreateEdgeButton("◀◀", LayoutOptions.Start, LayoutOptions.Center, () => Run(PreviousCommand, -1));
        _next = CreateEdgeButton("▶▶", LayoutOptions.End, LayoutOptions.Center, () => Run(NextCommand, 1));
        _playPause = CreateEdgeButton("▶❙❙", LayoutOptions.Center, LayoutOptions.End, () => Run(PlayPauseCommand, null));
        SemanticProperties.SetDescription(_previous, PreviousDescription);
        SemanticProperties.SetDescription(_next, NextDescription);
        SemanticProperties.SetDescription(_playPause, PlayPauseDescription);

        _center.SetDynamicResource(BackgroundColorProperty, "SurfaceColor");
        _center.BorderWidth = 2;
        _center.SetDynamicResource(ImageButton.BorderColorProperty, "DividerColor");
        _center.SetDynamicResource(OpenWave.App.Behaviors.IconTint.ColorProperty, "FavoriteIconColor");
        _center.HorizontalOptions = _center.VerticalOptions = LayoutOptions.Center;
        SemanticProperties.SetDescription(_center, CenterDescription);
        _center.Clicked += (_, _) => Press(_center, () => Run(CenterCommand, null));

        _ring.Children.Add(_menu);
        _ring.Children.Add(_previous);
        _ring.Children.Add(_next);
        _ring.Children.Add(_playPause);
        _ring.Children.Add(_center);

        _ring.HandlerChanged += (_, _) => AttachRingInput();
        HandlerChanged += (_, _) => AttachPageKeys();
        Content = _ring;
        HorizontalOptions = LayoutOptions.Center;
        UpdateRing();
    }

    public ICommand? ScrollStepCommand { get => (ICommand?)GetValue(ScrollStepCommandProperty); set => SetValue(ScrollStepCommandProperty, value); }

    public ICommand? CenterCommand { get => (ICommand?)GetValue(CenterCommandProperty); set => SetValue(CenterCommandProperty, value); }

    public ICommand? MenuCommand { get => (ICommand?)GetValue(MenuCommandProperty); set => SetValue(MenuCommandProperty, value); }

    public object? MenuCommandParameter { get => GetValue(MenuCommandParameterProperty); set => SetValue(MenuCommandParameterProperty, value); }

    /// <summary>
    /// False where MENÚ has nowhere to go (Inicio).
    /// </summary>
    public bool IsMenuEnabled { get => (bool)GetValue(IsMenuEnabledProperty); set => SetValue(IsMenuEnabledProperty, value); }

    /// <summary>
    /// Run with -1 (so a list can move a screen up).
    /// </summary>
    public ICommand? PreviousCommand { get => (ICommand?)GetValue(PreviousCommandProperty); set => SetValue(PreviousCommandProperty, value); }

    /// <summary>
    /// Run with 1 (so a list can move a screen down).
    /// </summary>
    public ICommand? NextCommand { get => (ICommand?)GetValue(NextCommandProperty); set => SetValue(NextCommandProperty, value); }

    public ICommand? PlayPauseCommand { get => (ICommand?)GetValue(PlayPauseCommandProperty); set => SetValue(PlayPauseCommandProperty, value); }

    /// <summary>
    /// False where sliding around the ring does nothing (Reproductor): the ring is then neither touchable nor read.
    /// </summary>
    public bool IsScrollEnabled { get => (bool)GetValue(IsScrollEnabledProperty); set => SetValue(IsScrollEnabledProperty, value); }

    public string PreviousDescription { get => (string)GetValue(PreviousDescriptionProperty); set => SetValue(PreviousDescriptionProperty, value); }

    public string NextDescription { get => (string)GetValue(NextDescriptionProperty); set => SetValue(NextDescriptionProperty, value); }

    public string PlayPauseDescription { get => (string)GetValue(PlayPauseDescriptionProperty); set => SetValue(PlayPauseDescriptionProperty, value); }

    public string CenterDescription { get => (string)GetValue(CenterDescriptionProperty); set => SetValue(CenterDescriptionProperty, value); }

    /// <summary>
    /// The centre button's icon (the heart in Reproductor); blank when not set.
    /// </summary>
    public ImageSource? CenterIcon { get => (ImageSource?)GetValue(CenterIconProperty); set => SetValue(CenterIconProperty, value); }

    private Button CreateEdgeButton(string text, LayoutOptions horizontal, LayoutOptions vertical, Action action)
    {
        var button = new Button
        {
            Text = text,
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            Padding = 0,
            WidthRequest = 64,
            HeightRequest = 52,
            HorizontalOptions = horizontal,
            VerticalOptions = vertical,
        };
        button.SetDynamicResource(Button.TextColorProperty, "TabBarUnselectedColor");
        button.SetDynamicResource(Button.FontFamilyProperty, "AppFontFamilyBold");
        button.FontAttributes = FontAttributes.Bold;
        button.Clicked += (_, _) => Press(button, action);
        return button;
    }

    // A short press feedback (80 ms), then the action.
    private static void Press(View button, Action action)
    {
        action();
        if (MotionSettings.IsReducedMotion)
        {
            return;
        }

        button.CancelAnimations();
        _ = button.ScaleToAsync(0.9, 40).ContinueWith(_ => button.Dispatcher.Dispatch(() => _ = button.ScaleToAsync(1, 40)));
    }

    private static void Run(ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    private void UpdateRing()
    {
        SemanticProperties.SetDescription(_ring, IsScrollEnabled ? "Rueda: desliza en círculo para moverte por la lista" : null);
        AutomationProperties.SetIsInAccessibleTree(_ring, IsScrollEnabled);
    }

    // The finger's position on the ring: each angle change around the centre is added up, one step every 24°.
    private void OnRingPointer(double x, double y, bool down, bool up)
    {
        if (!IsScrollEnabled || up)
        {
            _lastAngle = null;
            _turned = 0;
            return;
        }

        var angle = Math.Atan2(y - Size / 2, x - Size / 2) * 180 / Math.PI;
        if (down || _lastAngle is null)
        {
            _lastAngle = angle;
            return;
        }

        var delta = angle - _lastAngle.Value;
        delta = delta > 180 ? delta - 360 : delta < -180 ? delta + 360 : delta;
        _lastAngle = angle;
        _turned += delta;
        while (Math.Abs(_turned) >= DegreesPerStep)
        {
            var step = Math.Sign(_turned);
            _turned -= step * DegreesPerStep;
            Run(ScrollStepCommand, step);
        }
    }

    private void AttachRingInput()
    {
#if ANDROID
        if (_ring.Handler?.PlatformView is not Android.Views.View view)
        {
            return;
        }

        var density = view.Resources?.DisplayMetrics?.Density ?? 1;
        view.Touch += (_, e) =>
        {
            var motion = e.Event!;
            var action = motion.ActionMasked;
            OnRingPointer(motion.GetX() / density, motion.GetY() / density,
                action == Android.Views.MotionEventActions.Down,
                action is Android.Views.MotionEventActions.Up or Android.Views.MotionEventActions.Cancel);

            // Keep the gesture (and stop the page from scrolling) while the ring scrolls.
            e.Handled = IsScrollEnabled;
            if (IsScrollEnabled)
            {
                view.Parent?.RequestDisallowInterceptTouchEvent(true);
            }
        };
#elif WINDOWS
        if (_ring.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
        {
            return;
        }

        var pressed = false;
        view.PointerPressed += (_, e) =>
        {
            pressed = true;
            view.CapturePointer(e.Pointer);
            var p = e.GetCurrentPoint(view).Position;
            OnRingPointer(p.X, p.Y, true, false);
        };
        view.PointerMoved += (_, e) =>
        {
            if (pressed)
            {
                var p = e.GetCurrentPoint(view).Position;
                OnRingPointer(p.X, p.Y, false, false);
            }
        };
        view.PointerReleased += (_, e) =>
        {
            pressed = false;
            view.ReleasePointerCapture(e.Pointer);
            OnRingPointer(0, 0, false, true);
        };
#endif
    }

    // Windows: the page's keys for the wheel. Text boxes keep their keys.
    private void AttachPageKeys()
    {
#if WINDOWS
        Element? element = Parent;
        while (element is not null and not Page)
        {
            element = element.Parent;
        }

        if (element is not Page page)
        {
            return;
        }

        void Attach()
        {
            if (page.Handler?.PlatformView is not Microsoft.UI.Xaml.UIElement view)
            {
                return;
            }

            view.PreviewKeyDown += (_, e) =>
            {
                if (Handler is null || Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(view.XamlRoot) is Microsoft.UI.Xaml.Controls.TextBox)
                {
                    return;
                }

                switch (e.Key)
                {
                    case global::Windows.System.VirtualKey.Escape when IsMenuEnabled:
                        Run(MenuCommand, MenuCommandParameter);
                        e.Handled = true;
                        break;
                    case global::Windows.System.VirtualKey.Space:
                        Run(PlayPauseCommand, null);
                        e.Handled = true;
                        break;
                    case global::Windows.System.VirtualKey.Left:
                        Run(PreviousCommand, -1);
                        e.Handled = true;
                        break;
                    case global::Windows.System.VirtualKey.Right:
                        Run(NextCommand, 1);
                        e.Handled = true;
                        break;
                }
            };
        }

        if (page.Handler is not null)
        {
            Attach();
        }
        else
        {
            page.HandlerChanged += (_, _) => Attach();
        }
#endif
    }
}
