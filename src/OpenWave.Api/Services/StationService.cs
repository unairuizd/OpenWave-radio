using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Station catalogue backed by Radio Browser.
/// </summary>
public sealed class StationService(IRadioBrowserClient radioBrowserClient, ICatalogService catalogService) : IStationService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    // Radio Browser's tagList matches substrings, so many results are dropped by the exact-tag filter.
    private const int CombinedTagsFetchLimit = 300;

    private const string InvalidContentTypeMessage = "Tipo de contenido no válido.";

    public async Task<ServiceResult<IReadOnlyList<StationDto>>> GetStationsAsync(
        StationQuery query,
        CancellationToken cancellationToken = default)
    {
        var contentType = Clean(query.ContentType);
        if (contentType is not null)
        {
            var match = ContentTypes.All.FirstOrDefault(
                c => string.Equals(c.Value, contentType, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                return ServiceResult<IReadOnlyList<StationDto>>.Failure(
                    ServiceError.Invalid,
                    InvalidContentTypeMessage,
                    nameof(query.ContentType));
            }

            contentType = match.Value;
        }

        var name = Clean(query.Name);
        var limit = NormalizeLimit(query.Limit);

        // Radio Browser's exact matching is case-sensitive: use the catalogue's own spelling.
        var country = await CanonicalAsync(query.Country, catalogService.GetCountriesAsync, lowerCaseFallback: false, cancellationToken);
        var language = await CanonicalAsync(query.Language, catalogService.GetLanguagesAsync, lowerCaseFallback: true, cancellationToken);
        var tag = await CanonicalAsync(query.Tag, catalogService.GetGenresAsync, lowerCaseFallback: true, cancellationToken);

        var requiredTags = new List<string>();
        if (tag is not null)
        {
            requiredTags.Add(tag);
        }

        if (contentType is not null && !requiredTags.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            requiredTags.Add(contentType);
        }

        var fetchLimit = requiredTags.Count > 1 ? CombinedTagsFetchLimit : limit;
        var stations = await radioBrowserClient.SearchAsync(
            new RadioBrowserSearch(name, country, language, requiredTags, fetchLimit),
            cancellationToken);

        // Radio Browser matches names loosely and tag lists as substrings: keep only exact matches.
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var result = stations
            .Where(station => name is null || station.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .Where(station => HasAllTags(station, requiredTags))
            .Where(station => seenIds.Add(station.Id))
            .Take(limit)
            .ToList();

        return ServiceResult<IReadOnlyList<StationDto>>.Success(result);
    }

    /// <summary>
    /// Returns the option value equal to <paramref name="value"/> ignoring case, or the trimmed value
    /// (lower-cased for languages and tags, which the catalogue stores in lower case) when it is not listed.
    /// </summary>
    private static async Task<string?> CanonicalAsync(
        string? value,
        Func<CancellationToken, Task<IReadOnlyList<OptionDto>>> loadOptions,
        bool lowerCaseFallback,
        CancellationToken cancellationToken)
    {
        var cleaned = Clean(value);
        if (cleaned is null)
        {
            return null;
        }

        var options = await loadOptions(cancellationToken);
        var match = options.FirstOrDefault(o => string.Equals(o.Value, cleaned, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            return match.Value;
        }

        return lowerCaseFallback ? cleaned.ToLowerInvariant() : cleaned;
    }

    private static bool HasAllTags(StationDto station, IReadOnlyList<string> requiredTags)
    {
        if (requiredTags.Count == 0)
        {
            return true;
        }

        var tags = station.Tags ?? [];
        return requiredTags.All(required => tags.Contains(required, StringComparer.OrdinalIgnoreCase));
    }

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static int NormalizeLimit(int? limit)
    {
        if (limit is null or <= 0)
        {
            return DefaultLimit;
        }

        return Math.Min(limit.Value, MaxLimit);
    }
}
