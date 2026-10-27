using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Provides the radio station catalogue.
/// </summary>
public interface IStationService
{
    Task<IReadOnlyList<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default);
}
