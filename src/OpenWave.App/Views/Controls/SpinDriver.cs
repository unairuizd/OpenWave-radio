namespace OpenWave.App.Views.Controls;

/// <summary>
/// Turns something at a fixed speed with inertia: it speeds up and slows down over the given times, always from its
/// current angle. Only one dispatcher timer (about 16 ms) runs, and only while the thing moves; the owner applies each
/// angle step through the callback and decides when it is shown or in reduced motion.
/// </summary>
public sealed class SpinDriver
{
    private readonly IDispatcher _dispatcher;
    private readonly double _turnSpeed;
    private readonly double _spinUpSeconds;
    private readonly double _spinDownSeconds;
    private readonly Action<double> _rotateBy;

    private IDispatcherTimer? _timer;
    private DateTime _lastTick;
    private double _speed;
    private double _target;

    /// <param name="turnSpeed">Full speed in degrees per second.</param>
    /// <param name="rotateBy">Applies an angle step in degrees.</param>
    public SpinDriver(IDispatcher dispatcher, double turnSpeed, double spinUpSeconds, double spinDownSeconds, Action<double> rotateBy)
    {
        _dispatcher = dispatcher;
        _turnSpeed = turnSpeed;
        _spinUpSeconds = spinUpSeconds;
        _spinDownSeconds = spinDownSeconds;
        _rotateBy = rotateBy;
    }

    /// <summary>
    /// Speeds up to full speed, or slows down to a stop, from the current speed.
    /// </summary>
    public void SetMoving(bool moving)
    {
        _target = moving ? _turnSpeed : 0;
        Run();
    }

    /// <summary>
    /// Jumps to full speed or to a stop with no speed-up or slow-down: for a page coming back on screen, a hidden page
    /// and reduced motion.
    /// </summary>
    public void SnapToState(bool moving)
    {
        _target = moving ? _turnSpeed : 0;
        _speed = _target;
        Run();
    }

    private void Run()
    {
        if (_speed == 0 && _target == 0)
        {
            _timer?.Stop();
            return;
        }

        if (_timer is null)
        {
            _timer = _dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += OnTick;
        }

        if (!_timer.IsRunning)
        {
            _lastTick = DateTime.UtcNow;
            _timer.Start();
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var seconds = Math.Min(0.1, (now - _lastTick).TotalSeconds);
        _lastTick = now;

        var step = _turnSpeed * seconds / (_target > _speed ? _spinUpSeconds : _spinDownSeconds);
        _speed = _target > _speed ? Math.Min(_target, _speed + step) : Math.Max(_target, _speed - step);

        _rotateBy(_speed * seconds);

        if (_speed == 0 && _target == 0)
        {
            _timer?.Stop();
        }
    }
}
