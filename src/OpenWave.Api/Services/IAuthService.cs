using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Registers users and checks their credentials.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a user. Fails with <see cref="ServiceError.Invalid"/> or <see cref="ServiceError.Conflict"/>.
    /// </summary>
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Checks the credentials. Fails with <see cref="ServiceError.Invalid"/> or <see cref="ServiceError.Unauthorized"/>.
    /// </summary>
    Task<ServiceResult<UserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
