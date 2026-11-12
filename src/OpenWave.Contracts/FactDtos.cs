namespace OpenWave.Contracts;

/// <summary>
/// The music fact of one calendar day (UTC), in the language of the request.
/// </summary>
public sealed record FactDto(DateOnly Date, string Text);

/// <summary>
/// One music-related event or birth that happened on today's date, from Wikipedia's "On this day".
/// </summary>
public sealed record MusicEventDto(int Year, string Text, string ArticleUrl);

/// <summary>
/// Today's fact and the previous days' facts, newest first, plus what happened on today's date in music.
/// </summary>
public sealed record DailyFactsDto(
    DateOnly Date,
    IReadOnlyList<FactDto> Facts,
    IReadOnlyList<MusicEventDto> OnThisDay);
