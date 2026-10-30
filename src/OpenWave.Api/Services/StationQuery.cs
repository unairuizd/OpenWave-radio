namespace OpenWave.Api.Services;

/// <summary>
/// Optional station catalogue filters. Empty values are ignored.
/// </summary>
public sealed record StationQuery(
    string? Name = null,
    string? Country = null,
    string? Language = null,
    string? Tag = null,
    int? Limit = null,
    string? ContentType = null);
