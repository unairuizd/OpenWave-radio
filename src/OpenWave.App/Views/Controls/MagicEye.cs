using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// The green "magic eye" tuning indicator of the valve radio: a glowing disc with a dark wedge at the top. While
/// <see cref="IsActive"/> the two green sectors sweep towards each other and close the wedge over about 0.8 s; on stop
/// they open again in about 0.6 s. The final shape is shown at once with reduced motion and when its page is shown
/// again. Decorative: hidden from screen readers (the state is in the status text). Original art; colours from the
/// palette.
/// </summary>
public class MagicEye : PageAwareView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(MagicEye), false, propertyChanged: (b, _, _) => ((MagicEye)b).UpdateState(false));

    private const double OpenGap = 50;  // half-angle of the dark wedge, in degrees
    private const double ClosedGap = 4;
    private const string AnimationName = "MagicEyeSweep";

    private readonly Path _left = new();
    private readonly Path _right = new();
    private double _gap = OpenGap;

    public MagicEye()
    {
        var shadow = new Ellipse();
        shadow.SetDynamicResource(Shape.FillProperty, "InputBackgroundColor");
        shadow.SetDynamicResource(Shape.StrokeProperty, "DividerColor");
        shadow.StrokeThickness = 2;
        foreach (var sector in new[] { _left, _right })
        {
            sector.SetDynamicResource(Shape.FillProperty, "AccentColor");
        }

        var centre = new Ellipse { WidthRequest = 0, HeightRequest = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        centre.SetDynamicResource(Shape.FillProperty, "TabBarBackgroundColor");
        SizeChanged += (_, _) =>
        {
            centre.WidthRequest = centre.HeightRequest = Math.Min(Width, Height) * 0.3;
            Draw();
        };

        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        Content = new Grid { Children = { shadow, _left, _right, centre } };
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
        var target = IsActive ? ClosedGap : OpenGap;
        this.AbortAnimation(AnimationName);
        if (jumpToState || !IsShown || MotionSettings.IsReducedMotion)
        {
            _gap = target;
            Draw();
            return;
        }

        var start = _gap;
        new Animation(v => { _gap = v; Draw(); }, start, target, Easing.CubicInOut)
            .Commit(this, AnimationName, length: IsActive ? 800u : 600u);
    }

    // Each sector runs from the bottom up to its side of the wedge at the top.
    private void Draw()
    {
        var size = Math.Min(Width, Height);
        if (size <= 0)
        {
            return;
        }

        var r = size / 2 - 3;
        var centre = new Point(Width / 2, Height / 2);
        _right.Data = Sector(centre, r, _gap, 180);
        _left.Data = Sector(centre, r, 180, 360 - _gap);
    }

    // A pie slice from angle "from" to "to", in degrees measured clockwise from the top.
    private static Geometry Sector(Point centre, double r, double from, double to)
    {
        Point At(double degrees)
        {
            var radians = (degrees - 90) * Math.PI / 180;
            return new Point(centre.X + r * Math.Cos(radians), centre.Y + r * Math.Sin(radians));
        }

        var figure = new PathFigure { StartPoint = centre, IsClosed = true };
        figure.Segments.Add(new LineSegment(At(from)));
        figure.Segments.Add(new ArcSegment(At(to), new Size(r, r), 0, SweepDirection.Clockwise, to - from > 180));
        return new PathGeometry { Figures = { figure } };
    }
}
