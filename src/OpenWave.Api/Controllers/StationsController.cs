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
    /// <param name="country">Country filter forwarded to Radio Browser.</param>
    /// <param name="language">Language filter forwarded to Radio Browser.</param>
    /// <param name="tag">Tag filter forwarded to Radio Browser.</param>
    /// <param name="limit">Maximum number of stations: 50 by default, at most 100.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<StationDto>>> GetAsync(
        [FromQuery] string? name,
        [FromQuery] string? country,
        [FromQuery] string? language,
        [FromQuery] string? tag,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new StationQuery(name, country, language, tag, limit);
            var stations = await stationService.GetStationsAsync(query, cancellationToken);
            return Ok(stations);
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
