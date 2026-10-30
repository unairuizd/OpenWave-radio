using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Manages each user's favourite stations.
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Returns the user's favourites, newest first. Fails with <see cref="ServiceError.NotFound"/>.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<StationDto>>> ListAsync(long userId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a copy of the station and returns it as stored (long values shortened).
    /// Fails with <see cref="ServiceError.Invalid"/>, <see cref="ServiceError.NotFound"/> or <see cref="ServiceError.Conflict"/>.
    /// </summary>
    Task<ServiceResult<StationDto>> AddAsync(long userId, StationDto station, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the favourite. Fails with <see cref="ServiceError.NotFound"/>.
    /// </summary>
    Task<ServiceResult<bool>> RemoveAsync(long userId, string stationId, CancellationToken cancellationToken);
}
