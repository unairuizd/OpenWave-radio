using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Station catalogue backed by Radio Browser.
/// </summary>
public sealed class StationService(IRadioBrowserClient radioBrowserClient) : IStationService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    public async Task<IReadOnlyList<StationDto>> GetStationsAsync(
        StationQuery query,
        CancellationToken cancellationToken = default)
    {
        var name = query.Name?.Trim();
        var limit = NormalizeLimit(query.Limit);

        var stations = await radioBrowserClient.SearchAsync(
            name,
            query.Country,
            query.Language,
            query.Tag,
            limit,
            cancellationToken);

        // Radio Browser matches names loosely; keep only names that really contain the text.
        var hasName = !string.IsNullOrEmpty(name);
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        return stations
            .Where(station => !hasName || station.Name.Contains(name!, StringComparison.OrdinalIgnoreCase))
            .Where(station => seenIds.Add(station.Id))
            .Take(limit)
            .ToList();
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
