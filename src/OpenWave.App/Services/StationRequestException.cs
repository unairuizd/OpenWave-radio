namespace OpenWave.App.Services;

/// <summary>
/// Why a station request to the OpenWave API failed.
/// </summary>
public enum StationRequestError
{
    /// <summary>The API could not be reached or did not answer in time.</summary>
    Unreachable,

    /// <summary>The API answered 502: the external catalogue is unavailable.</summary>
    CatalogueUnavailable,

    /// <summary>Any other error status or a malformed response.</summary>
    Unexpected
}

/// <summary>
/// Thrown by <see cref="IStationService"/> when a station request fails.
/// </summary>
public sealed class StationRequestException(StationRequestError error, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public StationRequestError Error { get; } = error;
}
