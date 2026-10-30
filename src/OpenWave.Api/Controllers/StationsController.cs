using Microsoft.AspNetCore.Mvc;
using OpenWave.Api.Services;
using OpenWave.Contracts;

namespace OpenWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StationsController(
    IStationService stationService,
    ILogger<StationsController> logger) : ControllerBase
{
    private const string CatalogueUnavailableDetail = "No se pudo obtener el catálogo de emisoras.";

    /// <summary>
    /// Returns radio stations from the Radio Browser catalogue, most popular first.
    /// </summary>
    /// <param name="name">Text the station name must contain (case-insensitive).</param>
    /// <param name="country">Exact country name, ignoring case (values from <c>/api/catalog/countries</c>).</param>
    /// <param name="language">Exact language, ignoring case (values from <c>/api/catalog/languages</c>).</param>
    /// <param name="tag">Exact genre tag, ignoring case (values from <c>/api/catalog/genres</c>).</param>
    /// <param name="contentType">One of <c>music</c>, <c>news</c>, <c>talk</c> or <c>sports</c>.</param>
    /// <param name="limit">Maximum number of stations: 50 by default, at most 100.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<StationDto>>> GetAsync(
        [FromQuery] string? name,
        [FromQuery] string? country,
        [FromQuery] string? language,
        [FromQuery] string? tag,
        [FromQuery] string? contentType,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new StationQuery(name, country, language, tag, limit, contentType);
            var result = await stationService.GetStationsAsync(query, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
        }
        catch (RadioBrowserUnavailableException ex)
        {
            logger.LogError("Station catalogue unavailable: {Reason}", ex.InnerException?.Message ?? ex.Message);
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Bad Gateway",
                detail: CatalogueUnavailableDetail);
        }
    }
}
