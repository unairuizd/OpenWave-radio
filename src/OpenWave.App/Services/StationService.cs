using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for the station endpoints.
/// </summary>
public sealed class StationService(HttpClient httpClient) : IStationService
{
    private const int Limit = 50;

    public async Task<IReadOnlyList<StationDto>> SearchAsync(
        string? name,
        UserPreferences? filters = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/stations?limit={Limit}";
        requestUri += QueryPart("name", name?.Trim());
        requestUri += QueryPart("country", filters?.Country);
        requestUri += QueryPart("language", filters?.Language);
        requestUri += QueryPart("tag", filters?.Genre);
        requestUri += QueryPart("contentType", filters?.ContentType);

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
                var problem = await TryReadProblemAsync(response, cancellationToken);
                throw new StationRequestException(
                    StationRequestError.CatalogueUnavailable,
                    problem?.Detail ?? "The API reported that the catalogue is unavailable.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new StationRequestException(
                    StationRequestError.Unexpected,
                    $"The API answered {(int)response.StatusCode}.");
            }

            try
            {
                var stations = await response.Content.ReadFromJsonAsync<List<StationDto>>(cancellationToken);
                return stations ?? throw new JsonException("The response body was null.");
            }
            catch (JsonException ex)
            {
                throw new StationRequestException(StationRequestError.Unexpected, "The API response was malformed.", ex);
            }
        }
    }

    private static string QueryPart(string key, string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : $"&{key}={Uri.EscapeDataString(value)}";

    private static async Task<ProblemDetailsResponse?> TryReadProblemAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record ProblemDetailsResponse(string? Title, int? Status, string? Detail);
}
