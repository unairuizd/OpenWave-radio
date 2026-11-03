using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Brush = Microsoft.UI.Xaml.Media.Brush;
using ResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace OpenWave.App.WinUI;

/// <summary>
/// MAUI colours the window title with the Windows app mode (Settings > Personalisation > Colours), not with the app's
/// light or dark mode, so a light palette on a dark Windows showed white text on a light title bar. This sets the
/// title's text from the app's mode instead, dimmed while the window is inactive as MAUI does.
/// </summary>
internal static class TitleBarText
{
    public static void Attach(Microsoft.Maui.Controls.Window window)
    {
        if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native)
        {
            return;
        }

        var active = true;
        void Update()
        {
            if (native.Content is FrameworkElement root && FindTitle(root) is { } title)
            {
                var theme = Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Light ? "Light" : "Default";
                var key = active ? "TextFillColorPrimaryBrush" : "TextFillColorDisabledBrush";
                if (FindBrush(Microsoft.UI.Xaml.Application.Current.Resources, theme, key) is { } brush)
                {
                    title.Foreground = brush;
                }
            }
        }

        // After MAUI's own handler, which sets the title colour on every activation.
        native.Activated += (_, e) =>
        {
            active = e.WindowActivationState != WindowActivationState.Deactivated;
            native.DispatcherQueue.TryEnqueue(Update);
        };
        Microsoft.Maui.Controls.Application.Current!.RequestedThemeChanged += (_, _) => Update();
        Update();
    }

    // The title is the TextBlock named "AppTitle" in MAUI's default title bar.
    private static TextBlock? FindTitle(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBlock { Name: "AppTitle" } title)
            {
                return title;
            }

            if (FindTitle(child) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    // WinUI keeps the light brushes under "Light" and the dark ones under "Default", in a merged dictionary.
    private static Brush? FindBrush(ResourceDictionary dictionary, string theme, string key)
    {
        if (dictionary.ThemeDictionaries.TryGetValue(theme, out var themed)
            && themed is ResourceDictionary themedDictionary
            && themedDictionary.TryGetValue(key, out var value)
            && value is Brush brush)
        {
            return brush;
        }

        foreach (var merged in dictionary.MergedDictionaries)
        {
            if (FindBrush(merged, theme, key) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
