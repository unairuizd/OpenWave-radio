using System.Net.Http.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for the station endpoints.
/// </summary>
public sealed class StationService(HttpClient httpClient) : IStationService
{
    public async Task<IReadOnlyList<StationDto>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        var stations = await httpClient.GetFromJsonAsync<List<StationDto>>("api/stations", cancellationToken);
        return stations ?? [];
    }
}
