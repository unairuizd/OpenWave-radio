using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OpenWave.Api.Options;
using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Typed HTTP client for Radio Browser. Each mirror gets one attempt with its own timeout.
/// </summary>
public sealed class RadioBrowserClient(
    HttpClient httpClient,
    IOptions<RadioBrowserOptions> options,
    ILogger<RadioBrowserClient> logger) : IRadioBrowserClient
{
    public const string UserAgent = "OpenWave/1.0";

    private static readonly TimeSpan MirrorTimeout = TimeSpan.FromSeconds(5);

    public async Task<IReadOnlyList<StationDto>> SearchAsync(
        string? name,
        string? country,
        string? language,
        string? tag,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var path = BuildSearchPath(name, country, language, tag, limit);
        Exception? lastFailure = null;

        foreach (var server in GetServers())
        {
            var requestUri = new Uri(server, path);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(MirrorTimeout);

            try
            {
                using var response = await httpClient.GetAsync(requestUri, timeout.Token);
                response.EnsureSuccessStatusCode();

                var stations = await response.Content.ReadFromJsonAsync<List<RadioBrowserStation>>(timeout.Token)
                    ?? throw new JsonException("The response body was null instead of a JSON array.");

                return Map(stations);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or OperationCanceledException)
            {
                lastFailure = ex is OperationCanceledException
                    ? new TimeoutException($"No response within {MirrorTimeout.TotalSeconds} seconds.", ex)
                    : ex;

                logger.LogWarning("Radio Browser mirror {Server} failed: {Reason}", server, lastFailure.Message);
            }
        }

        logger.LogError(lastFailure, "Every Radio Browser mirror failed.");
        throw new RadioBrowserUnavailableException("Every Radio Browser mirror failed.", lastFailure);
    }

    private List<Uri> GetServers()
    {
        var servers = ParseServers(options.Value.Servers);
        if (servers.Count > 0)
        {
            return servers;
        }

        logger.LogWarning(
            "No valid {Section}:Servers configured; using the built-in mirror list.",
            RadioBrowserOptions.SectionName);

        return ParseServers(RadioBrowserOptions.DefaultServers);
    }

    private static List<Uri> ParseServers(IEnumerable<string> servers)
    {
        var result = new List<Uri>();

        foreach (var server in servers)
        {
            if (Uri.TryCreate(server?.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                result.Add(uri);
            }
        }

        return result;
    }

    private static string BuildSearchPath(string? name, string? country, string? language, string? tag, int limit)
    {
        var query = new List<KeyValuePair<string, string?>>
        {
            new("hidebroken", "true"),
            new("order", "clickcount"),
            new("reverse", "true"),
            new("limit", limit.ToString(CultureInfo.InvariantCulture))
        };

        AddIfPresent(query, "name", name);
        AddIfPresent(query, "country", country);
        AddIfPresent(query, "language", language);
        AddIfPresent(query, "tag", tag);

        return QueryHelpers.AddQueryString("json/stations/search", query);
    }

    private static void AddIfPresent(List<KeyValuePair<string, string?>> query, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add(new(key, value.Trim()));
        }
    }

    private static List<StationDto> Map(IEnumerable<RadioBrowserStation> stations)
    {
        var result = new List<StationDto>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var station in stations)
        {
            var id = Clean(station.StationUuid);
            var name = Clean(station.Name);
            var streamUrl = Clean(station.UrlResolved) ?? Clean(station.Url);

            if (id is null || name is null || streamUrl is null || !seenIds.Add(id))
            {
                continue;
            }

            result.Add(new StationDto
            {
                Id = id,
                Name = name,
                StreamUrl = streamUrl,
                Country = Clean(station.Country),
                Language = Clean(station.Language),
                Tags = SplitTags(station.Tags),
                LogoUrl = Clean(station.Favicon)
            });
        }

        return result;
    }

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string[]? SplitTags(string? tags)
    {
        var values = (tags ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return values.Length > 0 ? values : null;
    }
}
