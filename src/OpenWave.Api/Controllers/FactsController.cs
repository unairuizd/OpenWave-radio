using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenWave.Api.Services;
using OpenWave.Contracts;

namespace OpenWave.Api.Controllers;

[ApiController]
[Route("api/facts")]
public sealed class FactsController(IFactsService factsService, IStringLocalizer<Messages> localizer) : ControllerBase
{
    public const int DefaultDays = 15;
    public const int MaxDays = 31;

    /// <summary>
    /// Today's music fact and the previous days' facts, newest first, in the request language, plus what happened
    /// today in music according to Wikipedia.
    /// </summary>
    /// <param name="days">How many days, today included (1 to 31, 15 by default).</param>
    [HttpGet("daily")]
    [ProducesResponseType(typeof(DailyFactsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DailyFactsDto>> GetDailyAsync([FromQuery] int days = DefaultDays, CancellationToken cancellationToken = default)
    {
        if (days is < 1 or > MaxDays)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad Request", detail: localizer["InvalidDays"].Value);
        }

        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return Ok(await factsService.GetDailyAsync(days, language, cancellationToken));
    }
}
