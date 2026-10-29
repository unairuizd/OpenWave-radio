using System.Text.Json.Serialization;

namespace OpenWave.Api.Services;

/// <summary>
/// Station as returned by Radio Browser's <c>/json/stations/search</c>. Only the mapped fields are read.
/// </summary>
internal sealed record RadioBrowserStation
{
    [JsonPropertyName("stationuuid")]
    public string? StationUuid { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("url_resolved")]
    public string? UrlResolved { get; init; }

    [JsonPropertyName("url")]
    public string? Url { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("language")]
    public string? Language { get; init; }

    [JsonPropertyName("tags")]
    public string? Tags { get; init; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; init; }
}
