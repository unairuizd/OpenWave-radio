namespace OpenWave.Api.Services;

/// <summary>
/// One "on this day" item from Wikipedia: the year, the text and the article it links to.
/// </summary>
public sealed record OnThisDayItem(int Year, string Text, string ArticleUrl);

/// <summary>
/// Reads Wikipedia's "On this day" feed for a language and a date.
/// </summary>
public interface IWikimediaClient
{
    /// <summary>
    /// The events and the births of the day, in that order, unfiltered.
    /// </summary>
    /// <exception cref="HttpRequestException">Wikipedia answered an error.</exception>
    /// <exception cref="TaskCanceledException">Wikipedia did not answer within the timeout.</exception>
    Task<IReadOnlyList<OnThisDayItem>> GetOnThisDayAsync(string language, DateOnly date, CancellationToken cancellationToken = default);
}
