using System.Net.Http.Json;
using System.Text.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for the daily music facts (<c>/api/facts/daily</c>). The language travels in Accept-Language.
/// </summary>
public sealed class FactsApiClient(HttpClient httpClient)
{
    /// <exception cref="StationRequestException">The request failed (Unreachable or Unexpected).</exception>
    public async Task<DailyFactsDto> GetDailyAsync(int days, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/facts/daily?days={days}", cancellationToken);
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
            if (!response.IsSuccessStatusCode)
            {
                throw new StationRequestException(StationRequestError.Unexpected, $"The API answered {(int)response.StatusCode}.");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<DailyFactsDto>(cancellationToken)
                    ?? throw new JsonException("The response body was null.");
            }
            catch (JsonException ex)
            {
                throw new StationRequestException(StationRequestError.Unexpected, "The API response was malformed.", ex);
            }
        }
    }
}
