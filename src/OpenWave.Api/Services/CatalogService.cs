using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Option lists built from Radio Browser and kept in memory for an hour. Failed loads are not cached.
/// </summary>
public sealed class CatalogService(IRadioBrowserClient radioBrowserClient, IMemoryCache cache) : ICatalogService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    // Sorts "español" among the E's, ignoring case.
    private static readonly StringComparer LabelComparer = StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: true);

    public Task<IReadOnlyList<OptionDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        // Country names are already capitalised by the catalogue.
        return GetCachedAsync(
            "catalog:countries",
            radioBrowserClient.GetCountriesAsync,
            capitalise: false,
            minLength: 1,
            cancellationToken);
    }

    public Task<IReadOnlyList<OptionDto>> GetLanguagesAsync(CancellationToken cancellationToken = default)
    {
        return GetCachedAsync(
            "catalog:languages",
            radioBrowserClient.GetLanguagesAsync,
            capitalise: true,
            minLength: 1,
            cancellationToken);
    }

    public Task<IReadOnlyList<OptionDto>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        // Single-character tags are noise.
        return GetCachedAsync(
            "catalog:genres",
            radioBrowserClient.GetTagsAsync,
            capitalise: true,
            minLength: 2,
            cancellationToken);
    }

    public IReadOnlyList<OptionDto> GetContentTypes() => ContentTypes.All;

    private async Task<IReadOnlyList<OptionDto>> GetCachedAsync(
        string key,
        Func<CancellationToken, Task<IReadOnlyList<RadioBrowserNamedCount>>> load,
        bool capitalise,
        int minLength,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(key, out IReadOnlyList<OptionDto>? cached) && cached is not null)
        {
            return cached;
        }

        // A failure throws before Set, so it is never cached.
        var options = Map(await load(cancellationToken), capitalise, minLength);
        cache.Set(key, options, CacheDuration);
        return options;
    }

    private static IReadOnlyList<OptionDto> Map(IEnumerable<RadioBrowserNamedCount> entries, bool capitalise, int minLength)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var options = new List<OptionDto>();

        foreach (var entry in entries)
        {
            var name = entry.Name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length < minLength || entry.StationCount <= 0 || !seen.Add(name))
            {
                continue;
            }

            options.Add(new OptionDto(name, capitalise ? Capitalise(name) : name, entry.StationCount));
        }

        return options.OrderBy(o => o.Label, LabelComparer).ToList();
    }

    private static string Capitalise(string value) => char.ToUpperInvariant(value[0]) + value[1..];
}
