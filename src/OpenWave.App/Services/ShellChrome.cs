namespace OpenWave.App.Services;

/// <summary>
/// Makes the parts that the platform draws itself follow the chosen appearance: the radio button circles (platform
/// code in Platforms/). Every page hides the platform navigation bar and draws its own header, so nothing else is
/// left to the platform.
/// </summary>
public static partial class ShellChrome
{
    /// <summary>
    /// Colours the native radio button circles from the theme. Android paints them once, when the activity
    /// starts, so after a live theme switch they would otherwise keep the old theme's colour.
    /// </summary>
    public static void ApplyRadioColors()
    {
        var resources = Application.Current?.Resources;
        if (resources is null
            || !resources.TryGetValue("AccentColor", out var accent)
            || !resources.TryGetValue("SecondaryTextColor", out var secondary))
        {
            return;
        }

        ApplyPlatformRadioColors((Color)accent, (Color)secondary);
    }

    static partial void ApplyPlatformRadioColors(Color checkedColor, Color uncheckedColor);
}
