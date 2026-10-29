using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Queries the Radio Browser catalogue, trying the configured mirrors in order.
/// </summary>
public interface IRadioBrowserClient
{
    /// <summary>
    /// Searches stations ordered by click count. Empty filters are not sent.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">Every mirror failed.</exception>
    Task<IReadOnlyList<StationDto>> SearchAsync(
        string? name,
        string? country,
        string? language,
        string? tag,
        int limit,
        CancellationToken cancellationToken = default);
}
