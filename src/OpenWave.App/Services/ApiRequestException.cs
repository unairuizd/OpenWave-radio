using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OpenWave.App.Services.Localization;

namespace OpenWave.App.Services;

/// <summary>
/// Why a request to the account or favourites endpoints failed.
/// </summary>
public enum ApiErrorKind
{
    /// <summary>The API could not be reached or did not answer in time.</summary>
    Unreachable,

    /// <summary>503: the API could not reach the database.</summary>
    DatabaseUnavailable,

    /// <summary>400: the API rejected the input.</summary>
    Invalid,

    /// <summary>401: wrong email or password.</summary>
    Unauthorized,

    /// <summary>409: the data already exists.</summary>
    Conflict,

    /// <summary>404: the user or favourite does not exist.</summary>
    NotFound,

    /// <summary>Any other status or a malformed response.</summary>
    Unexpected
}

/// <summary>
/// Thrown by the account and favourites clients. <see cref="Exception.Message"/> is the text to show, in the app language.
/// </summary>
public sealed class ApiRequestException(ApiErrorKind kind, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public static string UnreachableMessage => Localizer.Instance["Error_Unreachable"];
    public static string DatabaseUnavailableMessage => Localizer.Instance["Error_DatabaseUnavailable"];
    public static string UnexpectedMessage => Localizer.Instance["Error_Unexpected"];

    public ApiErrorKind Kind { get; } = kind;

    /// <summary>
    /// Sends a request and turns network failures and timeouts into <see cref="ApiErrorKind.Unreachable"/>.
    /// </summary>
    public static async Task<HttpResponseMessage> SendAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        try
        {
            return await send(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // TaskCanceledException without caller cancellation is the HttpClient timeout.
            throw new ApiRequestException(ApiErrorKind.Unreachable, UnreachableMessage, ex);
        }
    }

    /// <summary>
    /// Builds the exception for an error response, keeping the server's Spanish <c>detail</c> where it is shown.
    /// </summary>
    public static async Task<ApiRequestException> FromResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var kind = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => ApiErrorKind.Invalid,
            HttpStatusCode.Unauthorized => ApiErrorKind.Unauthorized,
            HttpStatusCode.NotFound => ApiErrorKind.NotFound,
            HttpStatusCode.Conflict => ApiErrorKind.Conflict,
            HttpStatusCode.ServiceUnavailable => ApiErrorKind.DatabaseUnavailable,
            _ => ApiErrorKind.Unexpected,
        };

        var message = kind switch
        {
            ApiErrorKind.DatabaseUnavailable => DatabaseUnavailableMessage,
            ApiErrorKind.Unexpected => UnexpectedMessage,
            _ => await TryReadDetailAsync(response, cancellationToken) ?? UnexpectedMessage,
        };

        return new ApiRequestException(kind, message);
    }

    /// <summary>
    /// Reads a JSON body, turning a malformed one into <see cref="ApiErrorKind.Unexpected"/>.
    /// </summary>
    public static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken)
                ?? throw new JsonException("The response body was null.");
        }
        catch (JsonException ex)
        {
            throw new ApiRequestException(ApiErrorKind.Unexpected, UnexpectedMessage, ex);
        }
    }

    private static async Task<string?> TryReadDetailAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(cancellationToken);
            return string.IsNullOrWhiteSpace(problem?.Detail) ? null : problem.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record ProblemDetailsResponse(string? Detail);
}
