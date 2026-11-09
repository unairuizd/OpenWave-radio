using OpenWave.App.Services;

namespace OpenWave.App.Behaviors;

/// <summary>
/// Gives a CollectionView with a GridItemsLayout one column per <c>StationCardMinWidth</c> units of width (a size key
/// that follows the text size), and at least one: a card never gets narrower than its minimum. It changes Span on the existing layout, because Windows ignores
/// a new ItemsLayout assigned after the first render.
/// </summary>
public class AdaptiveSpanBehavior : Behavior<CollectionView>
{
    private const int MinSpan = 1;

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.SizeChanged += OnSizeChanged;
        _view = bindable;
        ComponentSizes.Changed += OnSizesChanged;
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        bindable.SizeChanged -= OnSizeChanged;
        ComponentSizes.Changed -= OnSizesChanged;
        _view = null;
        base.OnDetachingFrom(bindable);
    }

    private CollectionView? _view;

    private void OnSizesChanged(object? sender, EventArgs e) => OnSizeChanged(_view, e);

    private static void OnSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not CollectionView { ItemsLayout: GridItemsLayout layout } view || view.Width <= 0
            || Application.Current?.Resources.TryGetValue("StationCardMinWidth", out var value) != true || value is not double minWidth
            || minWidth <= 0)
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
