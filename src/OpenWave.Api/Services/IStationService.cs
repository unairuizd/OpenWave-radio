using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Provides the radio station catalogue.
/// </summary>
public interface IStationService
{
    /// <exception cref="RadioBrowserUnavailableException">The catalogue could not be reached.</exception>
    Task<IReadOnlyList<StationDto>> GetStationsAsync(StationQuery query, CancellationToken cancellationToken = default);
}
