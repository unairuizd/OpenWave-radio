namespace OpenWave.Api.Services;

/// <summary>
/// Thrown when every configured Radio Browser mirror failed.
/// </summary>
public sealed class RadioBrowserUnavailableException(string message, Exception? innerException)
    : Exception(message, innerException);
