namespace OpenWave.App.Views.Controls;

/// <summary>
/// Whether the operating system asks for reduced motion: animations turned off on Windows, or an animator duration
/// scale of 0 on Android (platform code in Platforms/).
/// </summary>
public static partial class MotionSettings
{
    public static bool IsReducedMotion
    {
        get
        {
            var reduced = false;
            ReadReducedMotion(ref reduced);
            return reduced;
        }
    }

    static partial void ReadReducedMotion(ref bool reduced);
}
