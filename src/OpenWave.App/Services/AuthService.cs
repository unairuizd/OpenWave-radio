using System.Net.Http.Json;
using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Typed HTTP client for <c>/api/auth</c>.
/// </summary>
public sealed class AuthService(HttpClient httpClient) : IAuthService
{
    public Task<UserDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return PostAsync("api/auth/login", new LoginRequest(email, password), cancellationToken);
    }

    public Task<UserDto> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return PostAsync("api/auth/register", new RegisterRequest(email, password), cancellationToken);
    }

    private async Task<UserDto> PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken)
    {
        using var response = await ApiRequestException.SendAsync(
            token => httpClient.PostAsJsonAsync(requestUri, body, token),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await ApiRequestException.FromResponseAsync(response, cancellationToken);
        }

        return await ApiRequestException.ReadAsync<UserDto>(response, cancellationToken);
    }
}
