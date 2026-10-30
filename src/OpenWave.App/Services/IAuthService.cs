using OpenWave.Contracts;

namespace OpenWave.App.Services;

/// <summary>
/// Account endpoints of the OpenWave API.
/// </summary>
public interface IAuthService
{
    /// <exception cref="ApiRequestException">The request failed; its message is the Spanish text to show.</exception>
    Task<UserDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <exception cref="ApiRequestException">The request failed; its message is the Spanish text to show.</exception>
    Task<UserDto> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
}
