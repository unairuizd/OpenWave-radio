namespace OpenWave.Api.Options;

/// <summary>
/// Radio Browser settings bound from the <c>RadioBrowser</c> configuration section.
/// </summary>
public sealed class RadioBrowserOptions
{
    public const string SectionName = "RadioBrowser";

    /// <summary>
    /// Mirrors used when the configured list is empty or has no valid URL.
    /// </summary>
    public static readonly IReadOnlyList<string> DefaultServers =
    [
        "https://de1.api.radio-browser.info",
        "https://all.api.radio-browser.info"
    ];

    /// <summary>
    /// Ordered list of mirror base URLs; they are tried in this order.
    /// </summary>
    public List<string> Servers { get; set; } = [];
}
