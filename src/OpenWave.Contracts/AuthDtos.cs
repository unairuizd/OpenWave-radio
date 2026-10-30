namespace OpenWave.Contracts;

/// <summary>
/// Data sent to create an account.
/// </summary>
public sealed record RegisterRequest(string Email, string Password);

/// <summary>
/// Data sent to log in.
/// </summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// A registered user as returned by the API. Never contains the password or its hash.
/// </summary>
public sealed record UserDto(long Id, string Email);
