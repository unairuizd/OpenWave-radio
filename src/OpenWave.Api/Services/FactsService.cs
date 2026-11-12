using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// Picks one fact per UTC day with a fixed rule, so every user sees the same fact on the same day and any past
/// day's fact can be recomputed, and adds today's music items from Wikipedia, cached until midnight UTC.
/// </summary>
public sealed partial class FactsService(
    FactsCatalog catalog,
    IWikimediaClient wikimediaClient,
    IMemoryCache cache,
    ILogger<FactsService> logger) : IFactsService
{
    /// <summary>Day zero of the rotation: the fact at position 0 is the fact of this day.</summary>
    public static readonly DateOnly Epoch = new(2026, 1, 1);

    /// <summary>At most this many "on this day" items are returned.</summary>
    public const int MaxOnThisDayItems = 3;

    public async Task<DailyFactsDto> GetDailyAsync(int days, string language, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var facts = new List<FactDto>(days);
        for (var i = 0; i < days; i++)
        {
            var date = today.AddDays(-i);
            facts.Add(new FactDto(date, catalog.GetText(IndexFor(date), language)));
        }

        var onThisDay = await GetOnThisDayAsync(language, today, cancellationToken);
        return new DailyFactsDto(today, facts, onThisDay);
    }

    /// <summary>The position of the fact of a day: days since the epoch, modulo the number of facts (never negative).</summary>
    public int IndexFor(DateOnly date)
    {
        var offset = (date.DayNumber - Epoch.DayNumber) % catalog.Count;
        return offset < 0 ? offset + catalog.Count : offset;
    }

    private async Task<IReadOnlyList<MusicEventDto>> GetOnThisDayAsync(string language, DateOnly today, CancellationToken cancellationToken)
    {
        var key = $"facts:onthisday:{language}:{today:yyyy-MM-dd}";
        if (cache.TryGetValue(key, out IReadOnlyList<MusicEventDto>? cached) && cached is not null)
        {
            return cached;
        }

        // A failure is cached too: the block stays empty for the rest of the day instead of waiting 5 seconds on
        // every request. The facts are returned either way.
        IReadOnlyList<MusicEventDto> items;
        try
        {
            var all = await wikimediaClient.GetOnThisDayAsync(language, today, cancellationToken);
            var keywords = KeywordsFor(language);
            items = all
                .Where(item => keywords.IsMatch(item.Text))
                .Take(MaxOnThisDayItems)
                .Select(item => new MusicEventDto(item.Year, item.Text, item.ArticleUrl))
                .ToList();
            logger.LogInformation("Wikipedia on this day ({Language}, {Date}): {Total} items, {Music} about music.", language, today, all.Count, items.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning("Wikipedia on this day unavailable ({Language}, {Date}): {Reason}", language, today, ex.Message);
            items = [];
        }

        var midnight = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        cache.Set(key, items, new MemoryCacheEntryOptions { AbsoluteExpiration = midnight });
        return items;
    }

    // Word starts ("music" also matches "musician", "músic" matches "música" and "músico"); the short ones are whole
    // words, and the ambiguous ones ("rock", "pop", "band") only count inside a musical phrase, so "Fastnet Rock",
    // "Pope" and "a band of rebels" are not music.
    private static Regex KeywordsFor(string language) =>
        language.Equals("en", StringComparison.OrdinalIgnoreCase) ? EnglishKeywords() : SpanishKeywords();

    [GeneratedRegex(
        @"(?<!\p{L})(músic|musical|cantante|cantautor|compositor|canci[oó]n|[áa]lbum|[óo]pera|concierto|guitarr|pianist|orquest|sinf[oó]n|violinist|bater[ií]a|discogr|rapero|sencillo|soprano|bar[ií]tono)"
        + @"|(?<!\p{L})(jazz|rap|tenor|disco|DJ)(?!\p{L})"
        + @"|(?<!\p{L})(banda (de rock|de pop|de m[uú]sica|de jazz|musical|sonora)|grupo (musical|de rock|de pop)|(m[uú]sica|estrella del|cantante de|[ií]dolo del) pop|rock and roll|rock ?'?n'? ?roll|(banda|grupo|cantante|guitarrista|estrella) de rock|punk rock|hard rock)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SpanishKeywords();

    [GeneratedRegex(
        @"(?<!\p{L})(music|singer|songwriter|composer|album|song|opera|concert|guitar|pianist|orchestra|symphon|violinist|drummer|bassist|rapper|record label|soprano|baritone|Grammy|Billboard)"
        + @"|(?<!\p{L})(jazz|rap|tenor|disco|DJ|hip hop)(?!\p{L})"
        + @"|(?<!\p{L})((rock|pop|jazz|boy|girl|big|brass|metal|punk) band|rock (music|group|star|singer|guitarist|album|opera)|rock and roll|rock ?'?n'? ?roll|punk rock|hard rock|pop (music|star|group|duo|album|idol))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EnglishKeywords();
}
