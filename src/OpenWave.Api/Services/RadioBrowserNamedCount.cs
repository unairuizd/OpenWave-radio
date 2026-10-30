using System.Text.Json.Serialization;

namespace OpenWave.Api.Services;

/// <summary>
/// An entry of Radio Browser's country, language or tag list. Only the mapped fields are read.
/// </summary>
public sealed record RadioBrowserNamedCount
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("stationcount")]
    public int StationCount { get; init; }
}
