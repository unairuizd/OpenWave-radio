namespace OpenWave.App.Services;

/// <summary>
/// Gives the app's single audio element a place in the window that belongs to no page, when the platform needs its
/// view to be shown for playback to start (platform code in Platforms/).
/// </summary>
public static partial class AudioHost
{
    public static void Attach(View element, Window window)
    {
        if (element.Handler?.PlatformView is { } platformView)
        {
            AddToWindow(platformView, window);
        }
    }

    static partial void AddToWindow(object platformView, Window window);
}
