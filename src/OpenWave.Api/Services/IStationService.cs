using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Provides the radio station catalogue.
/// </summary>
public interface IStationService
{
    /// <summary>
    /// Returns the matching stations. Fails with <see cref="ServiceError.Invalid"/> for an unknown content type.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">The catalogue could not be reached.</exception>
    Task<ServiceResult<IReadOnlyList<StationDto>>> GetStationsAsync(StationQuery query, CancellationToken cancellationToken = default);
}
