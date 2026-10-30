using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platform;

namespace OpenWave.App.Services;

public static partial class ShellChrome
{
    // On Android the tabs are a Material BottomNavigationView; its labels are TextViews inside each item.
    static partial void ApplyPlatformTabFonts(Shell shell, string fontFamily, double fontSize)
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window?.DecorView is not ViewGroup root || shell.Handler?.MauiContext is null)
        {
            return;
        }

        var fontManager = shell.Handler.MauiContext.Services.GetRequiredService<IFontManager>();
        var typeface = fontManager.GetTypeface(Microsoft.Maui.Font.OfSize(fontFamily, fontSize));

        foreach (var navigation in Descendants(root).OfType<BottomNavigationView>())
        {
            foreach (var label in Descendants(navigation).OfType<TextView>())
            {
                label.SetTypeface(typeface, Android.Graphics.TypefaceStyle.Normal);
                label.SetTextSize(ComplexUnitType.Sp, (float)fontSize);
            }
        }
    }

    static partial void ApplyPlatformRadioColors(Color checkedColor, Color uncheckedColor)
    {
        if (Platform.CurrentActivity?.Window?.DecorView is not ViewGroup root)
        {
            return;
        }

        var tint = new Android.Content.Res.ColorStateList(
            [[Android.Resource.Attribute.StateChecked], []],
            [checkedColor.ToPlatform().ToArgb(), uncheckedColor.ToPlatform().ToArgb()]);

        foreach (var radio in Descendants(root).OfType<Android.Widget.RadioButton>())
        {
            radio.ButtonTintList = tint;
        }
    }

    private static IEnumerable<Android.Views.View> Descendants(ViewGroup parent)
    {
        for (var i = 0; i < parent.ChildCount; i++)
        {
            var child = parent.GetChildAt(i);
            if (child is null)
            {
                continue;
            }

            yield return child;
            if (child is ViewGroup group)
            {
                foreach (var descendant in Descendants(group))
                {
                    yield return descendant;
                }
            }
        }
    }
}
