namespace OpenWave.App.Views.Controls;

/// <summary>
/// A view that knows whether its page is on screen, so animated controls can stop every timer and animation while
/// the page is hidden (another page shown, the app in the background, or a theme change removing the view).
/// </summary>
public abstract class PageAwareView : ContentView
{
    private Page? _page;

    protected PageAwareView()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// True while the view is in the visual tree and its page is shown.
    /// </summary>
    protected bool IsShown { get; private set; }

    /// <summary>
    /// Called when <see cref="IsShown"/> changes.
    /// </summary>
    protected virtual void OnShownChanged(bool shown)
    {
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _page = FindPage();
        if (_page is not null)
        {
            _page.Appearing += OnPageAppearing;
            _page.Disappearing += OnPageDisappearing;
        }

        SetShown(true);
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (_page is not null)
        {
            _page.Appearing -= OnPageAppearing;
            _page.Disappearing -= OnPageDisappearing;
            _page = null;
        }

        SetShown(false);
    }

    private void OnPageAppearing(object? sender, EventArgs e) => SetShown(true);

    private void OnPageDisappearing(object? sender, EventArgs e) => SetShown(false);

    private void SetShown(bool shown)
    {
        if (IsShown == shown)
        {
            return;
        }

        IsShown = shown;
        OnShownChanged(shown);
    }

    private Page? FindPage()
    {
        Element? element = Parent;
        while (element is not null and not Page)
        {
            element = element.Parent;
        }

        return element as Page;
    }
}
