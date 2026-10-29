using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Reads station data from the OpenWave API.
/// </summary>
public interface IStationService
{
    /// <summary>
    /// Returns the top stations, or the stations whose name contains <paramref name="name"/>.
    /// </summary>
    /// <exception cref="StationRequestException">The request failed.</exception>
    Task<IReadOnlyList<StationDto>> SearchAsync(string? name, CancellationToken cancellationToken = default);
}
