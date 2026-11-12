namespace OpenWave.Api.Options;

/// <summary>
/// Wikimedia Feed API settings bound from the <c>Wikimedia</c> configuration section.
/// </summary>
public sealed class WikimediaOptions
{
    public const string SectionName = "Wikimedia";

    /// <summary>
    /// Base URL of the feed, ending in a slash; the language code and <c>onthisday/...</c> are appended.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.wikimedia.org/feed/v1/wikipedia/";
}
