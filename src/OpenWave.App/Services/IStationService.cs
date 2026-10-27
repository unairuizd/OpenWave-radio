using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Reads station data from the OpenWave API.
/// </summary>
public interface IStationService
{
    Task<IReadOnlyList<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default);
}
