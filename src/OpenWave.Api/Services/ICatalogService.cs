using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// The option lists used to choose station filters.
/// </summary>
public interface ICatalogService
{
    /// <exception cref="RadioBrowserUnavailableException">The catalogue could not be reached.</exception>
    Task<IReadOnlyList<OptionDto>> GetCountriesAsync(CancellationToken cancellationToken = default);

    /// <exception cref="RadioBrowserUnavailableException">The catalogue could not be reached.</exception>
    Task<IReadOnlyList<OptionDto>> GetLanguagesAsync(CancellationToken cancellationToken = default);

    /// <exception cref="RadioBrowserUnavailableException">The catalogue could not be reached.</exception>
    Task<IReadOnlyList<OptionDto>> GetGenresAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The fixed content types; never contacts Radio Browser.
    /// </summary>
    IReadOnlyList<OptionDto> GetContentTypes();
}
