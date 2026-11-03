namespace OpenWave.App.Views.Controls;

/// <summary>
/// A decorative level meter of <see cref="Bars"/> bars of <see cref="Segments"/> segments. While
/// <see cref="IsActive"/> it moves on one 100 ms timer, with levels from a fixed pseudo-random sequence (no real audio),
/// smoothed; on stop it empties over about 400 ms. It stops while its page is hidden and, with reduced motion, shows a
/// fixed middle level while active. Hidden from screen readers and never focusable.
/// </summary>
public class LevelMeter : PageAwareView
{
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(LevelMeter), false, propertyChanged: (b, _, _) => ((LevelMeter)b).UpdateMotion());

    public static readonly BindableProperty BarsProperty = BindableProperty.Create(
        nameof(Bars), typeof(int), typeof(LevelMeter), 5, propertyChanged: (b, _, _) => ((LevelMeter)b).CreateSegments());

    public static readonly BindableProperty SegmentsProperty = BindableProperty.Create(
        nameof(Segments), typeof(int), typeof(LevelMeter), 8, propertyChanged: (b, _, _) => ((LevelMeter)b).CreateSegments());

    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation), typeof(StackOrientation), typeof(LevelMeter), StackOrientation.Vertical, propertyChanged: (b, _, _) => ((LevelMeter)b).CreateSegments());

    public static readonly BindableProperty LitColorProperty = BindableProperty.Create(
        nameof(LitColor), typeof(Color), typeof(LevelMeter), null, propertyChanged: (b, _, _) => ((LevelMeter)b).Draw());

    public static readonly BindableProperty PeakColorProperty = BindableProperty.Create(
        nameof(PeakColor), typeof(Color), typeof(LevelMeter), null, propertyChanged: (b, _, _) => ((LevelMeter)b).Draw());

    public static readonly BindableProperty UnlitColorProperty = BindableProperty.Create(
        nameof(UnlitColor), typeof(Color), typeof(LevelMeter), null, propertyChanged: (b, _, _) => ((LevelMeter)b).Draw());

    private const double MiddleLevel = 0.5;
    private const double EmptyStep = 0.25; // a full bar empties in four 100 ms ticks

    private readonly Grid _grid = new() { ColumnSpacing = 2, RowSpacing = 2 };
    private BoxView[][] _segments = [];
    private double[] _levels = [];
    private IDispatcherTimer? _timer;
    private uint _seed = 12345;

    public LevelMeter()
    {
        SetDynamicResource(LitColorProperty, "AccentColor");
        SetDynamicResource(UnlitColorProperty, "InputBackgroundColor");
        AutomationProperties.SetIsInAccessibleTree(this, false);
        InputTransparent = true;
        Content = _grid;
        CreateSegments();
    }

    /// <summary>
    /// True while a station is buffering or playing.
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public int Bars
    {
        get => (int)GetValue(BarsProperty);
        set => SetValue(BarsProperty, value);
    }

    public int Segments
    {
        get => (int)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    /// <summary>
    /// Vertical: the bars rise from the bottom. Horizontal: they grow from the left.
    /// </summary>
    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public Color? LitColor
    {
        get => (Color?)GetValue(LitColorProperty);
        set => SetValue(LitColorProperty, value);
    }

    /// <summary>
    /// The colour of the last segment when it is lit; the lit colour when not set.
    /// </summary>
    public Color? PeakColor
    {
        get => (Color?)GetValue(PeakColorProperty);
        set => SetValue(PeakColorProperty, value);
    }

    public Color? UnlitColor
    {
        get => (Color?)GetValue(UnlitColorProperty);
        set => SetValue(UnlitColorProperty, value);
    }

    protected override void OnShownChanged(bool shown) => UpdateMotion();

    private void CreateSegments()
    {
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        var vertical = Orientation == StackOrientation.Vertical;
        var bars = Math.Max(1, Bars);
        var segments = Math.Max(1, Segments);
        for (var i = 0; i < (vertical ? bars : segments); i++)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (var i = 0; i < (vertical ? segments : bars); i++)
        {
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        }

        _segments = new BoxView[bars][];
        for (var bar = 0; bar < bars; bar++)
        {
            _segments[bar] = new BoxView[segments];
            for (var segment = 0; segment < segments; segment++)
            {
                // Segment 0 is the bottom (vertical) or the left (horizontal) one.
                var box = new BoxView();
                Grid.SetColumn(box, vertical ? bar : segment);
                Grid.SetRow(box, vertical ? segments - 1 - segment : bar);
                _grid.Children.Add(box);
                _segments[bar][segment] = box;
            }
        }

        _levels = new double[bars];
        UpdateMotion();
    }

    private void UpdateMotion()
    {
        if (_segments.Length == 0)
        {
            return;
        }

        if (!IsShown)
        {
            // Hidden: nothing runs; back on screen it shows the current state at once.
            StopTimer();
            if (!IsActive)
            {
                Array.Fill(_levels, 0);
            }
        }
        else if (MotionSettings.IsReducedMotion)
        {
            StopTimer();
            Array.Fill(_levels, IsActive ? MiddleLevel : 0);
        }
        else if (IsActive || _levels.Any(l => l > 0))
        {
            StartTimer();
        }

        Draw();
    }

    private void StartTimer()
    {
        if (_timer is null)
        {
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += (_, _) => Tick();
        }

        if (!_timer.IsRunning)
        {
            _timer.Start();
        }
    }

    private void StopTimer() => _timer?.Stop();

    private void Tick()
    {
        var anyLit = false;
        for (var bar = 0; bar < _levels.Length; bar++)
        {
            if (IsActive)
            {
                // Halfway towards the next value of the sequence, so the bars move smoothly.
                var target = 0.2 + 0.8 * NextRandom();
                _levels[bar] += (target - _levels[bar]) / 2;
            }
            else
            {
                _levels[bar] = Math.Max(0, _levels[bar] - EmptyStep);
            }

            anyLit |= _levels[bar] > 0;
        }

        Draw();
        if (!IsActive && !anyLit)
        {
            StopTimer();
        }
    }

    // A fixed linear congruential sequence: the same "music" every time.
    private double NextRandom()
    {
        _seed = _seed * 1664525 + 1013904223;
        return (_seed >> 8) / (double)(1 << 24);
    }

    private void Draw()
    {
        for (var bar = 0; bar < _segments.Length; bar++)
        {
            var lit = (int)Math.Round(_levels[bar] * _segments[bar].Length);
            for (var segment = 0; segment < _segments[bar].Length; segment++)
            {
                var isPeak = segment == _segments[bar].Length - 1 && PeakColor is not null;
                _segments[bar][segment].Color = segment < lit ? (isPeak ? PeakColor : LitColor) : UnlitColor;
            }
        }
    }
}
