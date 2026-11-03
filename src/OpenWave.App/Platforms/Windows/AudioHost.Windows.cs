using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace OpenWave.App.Services;

public static partial class AudioHost
{
    // The Windows player only opens a source once its view is in a window, so the 1x1 view is added to the window's
    // root panel, next to (and under) the Shell. It is not part of any page.
    static partial void AddToWindow(object platformView, Microsoft.Maui.Controls.Window window)
    {
        if (platformView is not FrameworkElement view
            || window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window platformWindow
            || platformWindow.Content is not DependencyObject root
            || FindPanel(root) is not { } panel)
        {
            return;
        }

        view.Width = 1;
        view.Height = 1;
        view.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
        view.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top;
        view.IsHitTestVisible = false;
        panel.Children.Insert(0, view);
    }

    private static Panel? FindPanel(DependencyObject parent)
    {
        if (parent is Panel panel)
        {
            return panel;
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            if (FindPanel(VisualTreeHelper.GetChild(parent, i)) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
