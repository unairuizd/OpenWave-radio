using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for the API's filter option lists (<c>/api/catalog/...</c>).
/// </summary>
public sealed class CatalogApiClient(HttpClient httpClient)
{
    /// <exception cref="StationRequestException">The request failed.</exception>
    public Task<IReadOnlyList<OptionDto>> GetCountriesAsync(CancellationToken cancellationToken = default) =>
        GetAsync("api/catalog/countries", cancellationToken);

    /// <exception cref="StationRequestException">The request failed.</exception>
    public Task<IReadOnlyList<OptionDto>> GetLanguagesAsync(CancellationToken cancellationToken = default) =>
        GetAsync("api/catalog/languages", cancellationToken);

    /// <exception cref="StationRequestException">The request failed.</exception>
    public Task<IReadOnlyList<OptionDto>> GetGenresAsync(CancellationToken cancellationToken = default) =>
        GetAsync("api/catalog/genres", cancellationToken);

    /// <exception cref="StationRequestException">The request failed.</exception>
    public Task<IReadOnlyList<OptionDto>> GetContentTypesAsync(CancellationToken cancellationToken = default) =>
        GetAsync("api/catalog/content-types", cancellationToken);

    private async Task<IReadOnlyList<OptionDto>> GetAsync(string requestUri, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // TaskCanceledException without caller cancellation is the HttpClient timeout.
            throw new StationRequestException(StationRequestError.Unreachable, "The API could not be reached.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.BadGateway)
            {
                throw new StationRequestException(
                    StationRequestError.CatalogueUnavailable,
                    "The API reported that the catalogue is unavailable.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new StationRequestException(
                    StationRequestError.Unexpected,
                    $"The API answered {(int)response.StatusCode}.");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<List<OptionDto>>(cancellationToken)
                    ?? throw new JsonException("The response body was null.");
            }
            catch (JsonException ex)
            {
                throw new StationRequestException(StationRequestError.Unexpected, "The API response was malformed.", ex);
            }
        }
    }
}
