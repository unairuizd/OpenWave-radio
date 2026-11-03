namespace OpenWave.App.Views.Controls;

public static partial class MotionSettings
{
    // Settings > Accessibility > Visual effects > Animation effects.
    static partial void ReadReducedMotion(ref bool reduced)
    {
        reduced = !new global::Windows.UI.ViewManagement.UISettings().AnimationsEnabled;
    }
}
