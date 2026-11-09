using Microsoft.Maui.Layouts;

namespace OpenWave.App.Behaviors;

/// <summary>
/// Lays the children of a wrapping FlexLayout out in as many equal columns as fit: one column per
/// <see cref="MinItemWidthKey"/> units of width (a size key that follows the text size), between 1 and
/// <see cref="MaxColumns"/>. A card never gets narrower than its minimum, so its text never runs off the page.
/// </summary>
public class AdaptiveColumnsBehavior : Behavior<FlexLayout>
{
    public string MinItemWidthKey { get; set; } = "StationCardMinWidth";

    public int MaxColumns { get; set; } = 4;

    private FlexLayout? _layout;

    protected override void OnAttachedTo(FlexLayout bindable)
    {
        base.OnAttachedTo(bindable);
        _layout = bindable;
        bindable.SizeChanged += OnChanged;
        bindable.ChildAdded += OnChanged;
        // A text-size change resizes the cards, which invalidates the layout's measure.
        bindable.MeasureInvalidated += OnChanged;
        Services.ComponentSizes.Changed += OnChanged;
    }

    protected override void OnDetachingFrom(FlexLayout bindable)
    {
        bindable.SizeChanged -= OnChanged;
        bindable.ChildAdded -= OnChanged;
        bindable.MeasureInvalidated -= OnChanged;
        Services.ComponentSizes.Changed -= OnChanged;

        _layout = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnChanged(object? sender, EventArgs e) => Update();

    private void Update()
    {
        if (_layout is not { Width: > 0 } layout
            || Application.Current?.Resources.TryGetValue(MinItemWidthKey, out var value) != true || value is not double minWidth
            || minWidth <= 0)
        {
            return;
        }

        var columns = Math.Clamp((int)(layout.Width / minWidth), 1, Math.Max(1, MaxColumns));
        var basis = new FlexBasis(1f / columns, isRelative: true);
        var changed = false;
        foreach (var child in layout.Children.OfType<BindableObject>())
        {
            if (FlexLayout.GetBasis(child) != basis)
            {
                FlexLayout.SetBasis(child, basis);
                changed = true;
            }
        }

        // The number of rows changed: the layout and the page around it must measure again.
        if (changed)
        {
            ((IView)layout).InvalidateMeasure();
            (layout.Parent as IView)?.InvalidateMeasure();
        }
    }
}
