using System.Net.Http.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for <c>/api/users/{userId}/favorites</c>.
/// </summary>
public sealed class FavoritesApiClient(HttpClient httpClient)
{
    /// <exception cref="ApiRequestException">The request failed; its message is the Spanish text to show.</exception>
    public async Task<IReadOnlyList<StationDto>> ListAsync(long userId, CancellationToken cancellationToken = default)
    {
        using var response = await ApiRequestException.SendAsync(
            token => httpClient.GetAsync($"api/users/{userId}/favorites", token),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ApiRequestException.ReadAsync<List<StationDto>>(response, cancellationToken);
    }

    /// <exception cref="ApiRequestException">The request failed; its message is the Spanish text to show.</exception>
    public async Task<StationDto> AddAsync(long userId, StationDto station, CancellationToken cancellationToken = default)
    {
        using var response = await ApiRequestException.SendAsync(
            token => httpClient.PostAsJsonAsync($"api/users/{userId}/favorites", station, token),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await ApiRequestException.ReadAsync<StationDto>(response, cancellationToken);
    }

    /// <exception cref="ApiRequestException">The request failed; its message is the Spanish text to show.</exception>
    public async Task RemoveAsync(long userId, string stationId, CancellationToken cancellationToken = default)
    {
        using var response = await ApiRequestException.SendAsync(
            token => httpClient.DeleteAsync($"api/users/{userId}/favorites/{Uri.EscapeDataString(stationId)}", token),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await ApiRequestException.FromResponseAsync(response, cancellationToken);
        }
    }
}
