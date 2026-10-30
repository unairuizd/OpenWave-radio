using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Queries the Radio Browser catalogue, trying the configured mirrors in order.
/// </summary>
public interface IRadioBrowserClient
{
    /// <summary>
    /// Searches stations ordered by click count. Country, language and tags match exactly; empty filters are not sent.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">Every mirror failed.</exception>
    Task<IReadOnlyList<StationDto>> SearchAsync(RadioBrowserSearch search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every country with its number of working stations.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">Every mirror failed.</exception>
    Task<IReadOnlyList<RadioBrowserNamedCount>> GetCountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the 100 languages with the most working stations.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">Every mirror failed.</exception>
    Task<IReadOnlyList<RadioBrowserNamedCount>> GetLanguagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the 100 tags with the most working stations.
    /// </summary>
    /// <exception cref="RadioBrowserUnavailableException">Every mirror failed.</exception>
    Task<IReadOnlyList<RadioBrowserNamedCount>> GetTagsAsync(CancellationToken cancellationToken = default);
}
