using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenWave.Api.Options;

namespace OpenWave.Api.Services;

/// <summary>
/// Typed client for the public Wikimedia Feed API: <c>{BaseUrl}{language}/onthisday/{type}/{MM}/{DD}</c>.
/// Wikimedia asks every client to identify itself, hence the User-Agent set in Program.cs.
/// </summary>
public sealed class WikimediaClient(HttpClient httpClient, IOptions<WikimediaOptions> options) : IWikimediaClient
{
    public const string UserAgent = "OpenWave/1.0 (course project)";

    /// <summary>The whole call (both feeds) must finish within this time; otherwise the day has no items.</summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

    public async Task<IReadOnlyList<OnThisDayItem>> GetOnThisDayAsync(
        string language,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var items = new List<OnThisDayItem>();
        foreach (var type in new[] { "events", "births" })
        {
            items.AddRange(await GetAsync(language, date, type, cancellationToken));
        }

        return items;
    }

    private async Task<IEnumerable<OnThisDayItem>> GetAsync(
        string language,
        DateOnly date,
        string type,
        CancellationToken cancellationToken)
    {
        // The feed wants a zero-padded month and day.
        var baseUri = new Uri(options.Value.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        var requestUri = new Uri(baseUri, FormattableString.Invariant($"{language}/onthisday/{type}/{date.Month:00}/{date.Day:00}"));

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var feed = await response.Content.ReadFromJsonAsync<Feed>(JsonOptions, cancellationToken)
            ?? throw new JsonException("The response body was empty.");
        var entries = (type == "events" ? feed.Events : feed.Births) ?? [];

        return entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Text) && e.Year is not null)
            .Select(e => new OnThisDayItem(e.Year!.Value, e.Text!.Trim(), ArticleFor(e)));
    }

    // The feed links every page the text mentions, generic ones first ("Cantautor" before "Taylor Swift"): the
    // page whose title appears in the text and is longest is the subject; otherwise the first page.
    private static string ArticleFor(Entry entry)
    {
        var pages = entry.Pages ?? [];
        var subject = pages
            .Where(p => !string.IsNullOrEmpty(p.Titles?.Normalized)
                && entry.Text!.Contains(p.Titles.Normalized, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Titles!.Normalized!.Length)
            .FirstOrDefault() ?? pages.FirstOrDefault();
        return subject?.ContentUrls?.Desktop?.Page ?? string.Empty;
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // Only the fields the API uses; the feed has many more.
    private sealed record Feed(List<Entry>? Events, List<Entry>? Births);

    private sealed record Entry(string? Text, int? Year, List<Page>? Pages);

    private sealed record Page(Titles? Titles, [property: JsonPropertyName("content_urls")] ContentUrls? ContentUrls);

    private sealed record Titles(string? Normalized);

    private sealed record ContentUrls(UrlSet? Desktop);

    private sealed record UrlSet(string? Page);
}
