namespace OpenWave.App.Behaviors;

/// <summary>
/// Gives a CollectionView with a GridItemsLayout one column per <c>CardMinWidth</c> units of width (the layout
/// token in Styles.xaml), and never fewer than two. It changes Span on the existing layout, because Windows ignores
/// a new ItemsLayout assigned after the first render.
/// </summary>
public class AdaptiveSpanBehavior : Behavior<CollectionView>
{
    private const int MinSpan = 2;

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.SizeChanged += OnSizeChanged;
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        bindable.SizeChanged -= OnSizeChanged;
        base.OnDetachingFrom(bindable);
    }

    private static void OnSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not CollectionView { ItemsLayout: GridItemsLayout layout } view || view.Width <= 0
            || Application.Current?.Resources.TryGetValue("CardMinWidth", out var value) != true || value is not double minWidth)
        {
            return;
        }

        var span = Math.Max(MinSpan, (int)(view.Width / minWidth));
        if (layout.Span != span)
        {
            layout.Span = span;
        }
    }
}
