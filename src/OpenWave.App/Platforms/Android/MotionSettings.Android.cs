namespace OpenWave.App.Views.Controls;

public static partial class MotionSettings
{
    // Developer options > Animator duration scale (also set to 0 by "Remove animations" in Accessibility).
    static partial void ReadReducedMotion(ref bool reduced)
    {
        var resolver = Android.App.Application.Context.ContentResolver;
        reduced = Android.Provider.Settings.Global.GetFloat(resolver, Android.Provider.Settings.Global.AnimatorDurationScale, 1f) == 0f;
    }
}
