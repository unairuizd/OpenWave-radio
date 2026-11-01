using Android.Views;
using Android.Widget;
using Microsoft.Maui.Platform;

namespace OpenWave.App.Services;

public static partial class ShellChrome
{
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
