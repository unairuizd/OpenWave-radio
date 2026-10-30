using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

namespace OpenWave.App.Services;

public static partial class ShellChrome
{
    // On Windows the tabs are NavigationViewItems; their text inherits the item's font.
    static partial void ApplyPlatformTabFonts(Shell shell, string fontFamily, double fontSize)
    {
        if (shell.Handler?.PlatformView is not DependencyObject root || shell.Handler.MauiContext is null)
        {
            return;
        }

        var fontManager = shell.Handler.MauiContext.Services.GetRequiredService<IFontManager>();
        var family = fontManager.GetFontFamily(Microsoft.Maui.Font.OfSize(fontFamily, fontSize));

        foreach (var item in Descendants(root).OfType<WinNavigationViewItem>())
        {
            item.FontFamily = family;
            item.FontSize = fontSize;
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
