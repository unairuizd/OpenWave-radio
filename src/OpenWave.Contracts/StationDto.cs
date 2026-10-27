namespace OpenWave.Contracts;

/// <summary>
/// Station data exchanged between the API and the app.
/// </summary>
public sealed record StationDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string StreamUrl { get; init; }

    public string? Country { get; init; }

    public string? Language { get; init; }

    public string[]? Tags { get; init; }

    public string? LogoUrl { get; init; }
}
