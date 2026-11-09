namespace OpenWave.App.Services;

/// <summary>
/// The size limits of the components that contain text, at the <c>Normal</c> text size. <see cref="Apply"/> writes
/// each one into the app resources multiplied by the text-size factor, so a larger text size gives those components
/// more room. A minimum width is never larger than the window minus the page padding, so no page scrolls sideways.
/// The limits of drawn pieces do not follow the text size: they are constants in Resources/Styles/Styles.xaml.
/// </summary>
public static class ComponentSizes
{
    /// <summary>Horizontal padding of the page content, on both sides together.</summary>
    public const double PagePadding = 32;

    // Key, Normal value, and whether it is a minimum width (capped at the window width).
    private static readonly (string Key, double Value, bool IsMinWidth)[] Sizes =
    [
        ("ChromeMinHeight", 56, false),
        ("ChromeMaxHeight", 80, false),
        ("StationCardMinWidth", 150, true),
        ("StationCardMaxWidth", 220, false),
        ("StationRowMinWidth", 280, true),
        ("StationRowMaxWidth", 720, false),
        ("StationRowMinHeight", 48, false),
        ("StationRowMaxHeight", 96, false),
        ("PlayerPanelMinWidth", 280, true),
        ("PlayerPanelMaxWidth", 560, false),
        ("SettingsColumnMinWidth", 300, true),
        ("SettingsColumnMaxWidth", 720, false),
        ("SettingsRowMinHeight", 48, false),
        ("SettingsRowMaxHeight", 120, false),
        ("FormMinWidth", 280, true),
        ("FormMaxWidth", 480, false),
        ("KeyMinWidth", 48, false),
        ("KeyMaxWidth", 160, false),
        ("KeyMinHeight", 48, false),
        ("KeyMaxHeight", 72, false),
        ("ListMaxHeight", 560, false),
    ];

    /// <summary>Raised after the keys were rewritten (text size or window width changed).</summary>
    public static event EventHandler? Changed;

    /// <summary>The Normal value of a key, for code that needs it before the resources exist.</summary>
    public static double Normal(string key) => Sizes.First(s => s.Key == key).Value;

    /// <summary>Writes every key for this text-size factor and window width (infinity when not known yet).</summary>
    public static void Apply(ResourceDictionary resources, double scale, double windowWidth)
    {
        var widest = double.IsFinite(windowWidth) ? Math.Max(0, windowWidth - PagePadding) : double.PositiveInfinity;
        foreach (var (key, value, isMinWidth) in Sizes)
        {
            var scaled = Math.Round(value * scale, 1);
            resources[key] = isMinWidth ? Math.Min(scaled, widest) : scaled;
        }

        Changed?.Invoke(null, EventArgs.Empty);
    }
}
