using Microsoft.Maui.Layouts;

namespace OpenWave.App.Views.Controls;

/// <summary>
/// Hosts a component with text (a column of rows, a form, a player panel): it takes the width it is given, keeps it
/// between <see cref="MinWidth"/> and <see cref="MaxWidth"/> and centres its child in the space it gets. The height
/// is whatever the child needs, so the text always fits and the page scrolls when it does not. The limits are bound
/// to the size keys that follow the text size.
/// </summary>
public class SizeLimit : Layout
{
    public static readonly BindableProperty MinWidthProperty =
        BindableProperty.Create(nameof(MinWidth), typeof(double), typeof(SizeLimit), 0.0, propertyChanged: Invalidate);

    public static readonly BindableProperty MaxWidthProperty =
        BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(SizeLimit), double.PositiveInfinity, propertyChanged: Invalidate);

    // A child that scrolls by itself (a list) gets the full available height instead of its natural height.
    public static readonly BindableProperty FillHeightProperty =
        BindableProperty.Create(nameof(FillHeight), typeof(bool), typeof(SizeLimit), false, propertyChanged: Invalidate);

    public bool FillHeight
    {
        get => (bool)GetValue(FillHeightProperty);
        set => SetValue(FillHeightProperty, value);
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

    // The minimum never makes the component wider than the space it gets: in a small window at a large text size the
    // text wraps instead of the page scrolling sideways.
    private double WidthFor(double available)
    {
        if (!double.IsFinite(available))
        {
            return double.IsFinite(MaxWidth) ? MaxWidth : MinWidth;
        }

        return Math.Max(Math.Min(MinWidth, available), Math.Min(MaxWidth, available));
    }

    protected override ILayoutManager CreateLayoutManager() => new Manager(this);

    private static void Invalidate(BindableObject bindable, object oldValue, object newValue) =>
        ((SizeLimit)bindable).InvalidateMeasure();

    private sealed class Manager(SizeLimit box) : ILayoutManager
    {
        public Size Measure(double widthConstraint, double heightConstraint)
        {
            var width = box.WidthFor(widthConstraint - box.Padding.HorizontalThickness);
            var height = 0.0;
            foreach (var child in box)
            {
                var childHeight = box.FillHeight ? heightConstraint - box.Padding.VerticalThickness : double.PositiveInfinity;
                height = Math.Max(height, child.Measure(width, childHeight).Height);
            }

            if (box.FillHeight && double.IsFinite(heightConstraint))
            {
                height = heightConstraint - box.Padding.VerticalThickness;
            }

            return new Size(width + box.Padding.HorizontalThickness, height + box.Padding.VerticalThickness);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            var inner = bounds.Width - box.Padding.HorizontalThickness;
            var width = box.WidthFor(inner);
            var x = bounds.X + box.Padding.Left + Math.Max(0, (inner - width) / 2);
            var height = bounds.Height - box.Padding.VerticalThickness;
            foreach (var child in box)
            {
                child.Arrange(new Rect(x, bounds.Y + box.Padding.Top, width, height));
            }

            return bounds.Size;
        }
    }
}
