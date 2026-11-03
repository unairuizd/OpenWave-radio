namespace OpenWave.App.Views.Controls;

/// <summary>
/// A one-line label that scrolls sideways when its text is wider than the box: about 40 units per second, pausing
/// about 1 s at each end, only while its page is shown. Text that fits stays still. With reduced motion it does not
/// scroll: the text wraps to two lines ending with "…". Screen readers read the full text once, from this control;
/// the moving copy is hidden from them.
/// </summary>
public class MarqueeLabel : PageAwareView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(MarqueeLabel), null, propertyChanged: (b, _, _) => ((MarqueeLabel)b).OnTextChanged());

    public static readonly BindableProperty LabelStyleProperty = BindableProperty.Create(
        nameof(LabelStyle), typeof(Style), typeof(MarqueeLabel), null, propertyChanged: (b, _, n) => ((MarqueeLabel)b)._label.Style = (Style?)n);

    private const double Speed = 40; // units per second
    private const int EndPauseMilliseconds = 1000;

    private readonly Label _label = new() { LineBreakMode = LineBreakMode.NoWrap, MaxLines = 1, HorizontalOptions = LayoutOptions.Start };
    private CancellationTokenSource? _scroll;

    public MarqueeLabel()
    {
        AutomationProperties.SetIsInAccessibleTree(_label, false);
        IsClippedToBounds = true;

        // A horizontal stack lets the label take its full text width, so it can slide inside the clipped box.
        Content = new HorizontalStackLayout { Children = { _label } };
        SizeChanged += (_, _) => Restart();
    }

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// The style of the text (a named label style of the layout).
    /// </summary>
    public Style? LabelStyle
    {
        get => (Style?)GetValue(LabelStyleProperty);
        set => SetValue(LabelStyleProperty, value);
    }

    protected override void OnShownChanged(bool shown) => Restart();

    private void OnTextChanged()
    {
        _label.Text = Text;
        SemanticProperties.SetDescription(this, Text);
        Restart();
    }

    private void Restart()
    {
        _scroll?.Cancel();
        _scroll = null;
        _label.CancelAnimations();
        _label.TranslationX = 0;

        if (MotionSettings.IsReducedMotion)
        {
            // Still, on two lines with an ellipsis, as wide as the box.
            _label.LineBreakMode = LineBreakMode.TailTruncation;
            _label.MaxLines = 2;
            _label.WidthRequest = Math.Max(0, Width);
            return;
        }

        _label.LineBreakMode = LineBreakMode.NoWrap;
        _label.MaxLines = 1;
        _label.WidthRequest = -1;
        if (!IsShown || Width <= 0 || string.IsNullOrEmpty(Text))
        {
            return;
        }

        var textWidth = _label.Measure(double.PositiveInfinity, double.PositiveInfinity).Width;
        var overflow = textWidth - Width;
        if (overflow <= 0)
        {
            return;
        }

        _scroll = new CancellationTokenSource();
        _ = ScrollAsync(overflow, _scroll.Token);
    }

    // Left to the end, pause, back to the start, pause, while shown.
    private async Task ScrollAsync(double overflow, CancellationToken token)
    {
        var duration = (uint)(overflow / Speed * 1000);
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(EndPauseMilliseconds, token);
                await _label.TranslateToAsync(-overflow, 0, duration, Easing.Linear);
                await Task.Delay(EndPauseMilliseconds, token);
                await _label.TranslateToAsync(0, 0, duration, Easing.Linear);
            }
        }
        catch (TaskCanceledException)
        {
            // Hidden, resized or new text: Restart takes over.
        }
    }
}
