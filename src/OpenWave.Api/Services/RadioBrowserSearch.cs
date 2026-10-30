namespace OpenWave.Api.Services;

/// <summary>
/// A station search sent to Radio Browser. Country, language and tags must already use the catalogue's
/// own spelling, because Radio Browser's exact matching is case-sensitive.
/// </summary>
/// <param name="Name">Text the name must contain; matched loosely by Radio Browser.</param>
/// <param name="Country">Exact country name.</param>
/// <param name="Language">Exact language name.</param>
/// <param name="Tags">Tags every station must have; zero, one or two.</param>
/// <param name="Limit">Maximum number of stations to request.</param>
public sealed record RadioBrowserSearch(
    string? Name,
    string? Country,
    string? Language,
    IReadOnlyList<string> Tags,
    int Limit);
