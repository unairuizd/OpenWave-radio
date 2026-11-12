using OpenWave.Contracts;

namespace OpenWave.Api.Services;

/// <summary>
/// The music fact of each day and what happened today in music.
/// </summary>
public interface IFactsService
{
    /// <summary>
    /// Today's fact (UTC) and the facts of the previous <paramref name="days"/> − 1 days, newest first, in the given
    /// language, plus today's music items from Wikipedia (empty when Wikipedia has none or cannot be reached).
    /// </summary>
    Task<DailyFactsDto> GetDailyAsync(int days, string language, CancellationToken cancellationToken = default);
}
