namespace OpenWave.Api.Data.Entities;

/// <summary>
/// A row of the <c>favorite_stations</c> table: a copy of the station data saved by one user.
/// </summary>
public sealed class FavoriteStationEntity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    /// <summary>
    /// The station id (<c>StationDto.Id</c>), unique per user.
    /// </summary>
    public required string StationUuid { get; set; }

    public required string Name { get; set; }

    public required string StreamUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? Country { get; set; }

    public string? Language { get; set; }

    /// <summary>
    /// Comma-separated tags.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// UTC time the favourite was added; orders the list, newest first.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
