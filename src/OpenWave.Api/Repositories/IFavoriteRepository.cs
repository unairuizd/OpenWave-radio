using OpenWave.Api.Data.Entities;

namespace OpenWave.Api.Repositories;

/// <summary>
/// Data access for the <c>favorite_stations</c> table.
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>
    /// Returns the user's favourites, newest first.
    /// </summary>
    Task<IReadOnlyList<FavoriteStationEntity>> ListByUserAsync(long userId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(long userId, string stationUuid, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the favourite and saves. A duplicate (user, station) pair fails with the database's unique-key error.
    /// </summary>
    Task AddAsync(FavoriteStationEntity favorite, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the favourite; returns <c>false</c> when the user has no such favourite.
    /// </summary>
    Task<bool> RemoveAsync(long userId, string stationUuid, CancellationToken cancellationToken);
}
