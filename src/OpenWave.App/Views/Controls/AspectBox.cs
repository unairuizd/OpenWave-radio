using Microsoft.Maui.Layouts;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// Hosts a drawn piece with a fixed shape: it takes the width it is given, keeps it between
/// <see cref="MinWidth"/> and <see cref="MaxWidth"/>, makes its height <c>width / Ratio</c> and centres itself in
/// the space it gets. Its children fill it. The size never depends on the height of the space around it, so a
/// drawn piece keeps its shape inside a vertical scroll area.
/// </summary>
public class AspectBox : Layout
{
    public static readonly BindableProperty RatioProperty =
        BindableProperty.Create(nameof(Ratio), typeof(double), typeof(AspectBox), 1.0, propertyChanged: Invalidate);

    public static readonly BindableProperty MinWidthProperty =
        BindableProperty.Create(nameof(MinWidth), typeof(double), typeof(AspectBox), 0.0, propertyChanged: Invalidate);

    public static readonly BindableProperty MaxWidthProperty =
        BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(AspectBox), double.PositiveInfinity, propertyChanged: Invalidate);

    /// <summary>Width divided by height.</summary>
    public double Ratio
    {
        get => (double)GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    public double MinWidth
    {
        get => (double)GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    /// <summary>The width the box takes when it is offered <paramref name="available"/> units.</summary>
    public double WidthFor(double available)
    {
        var max = double.IsFinite(MaxWidth) ? MaxWidth : Math.Max(MinWidth, 0);
        var width = double.IsFinite(available) ? available : max;
        return Math.Max(MinWidth, Math.Min(max, width));
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue) =>
        ((AspectBox)bindable).InvalidateMeasure();

    private sealed class Manager(AspectBox box) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var width = box.WidthFor(widthConstraint - box.Padding.HorizontalThickness);
            var height = width / Math.Max(box.Ratio, 0.01);
            foreach (var child in box)
            {
                child.Measure(width, height);
            }

            return new Size(width + box.Padding.HorizontalThickness, height + box.Padding.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var inner = bounds.Width - box.Padding.HorizontalThickness;
            var width = box.WidthFor(inner);
            var height = width / Math.Max(box.Ratio, 0.01);
            var x = bounds.X + box.Padding.Left + Math.Max(0, (inner - width) / 2);
            var rect = new Rect(x, bounds.Y + box.Padding.Top, width, height);
            foreach (var child in box)
            {
                child.Arrange(rect);
            }

            return new Size(bounds.Width, height + box.Padding.VerticalThickness);
        }
    }
}
