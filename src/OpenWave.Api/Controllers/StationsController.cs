using Microsoft.AspNetCore.Mvc;
using OpenWave.Api.Services;
using OpenWave.Contracts;

namespace OpenWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StationsController(IStationService stationService) : ControllerBase
{
    /// <summary>
    /// Returns the available radio stations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StationDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var stations = await stationService.GetStationsAsync(cancellationToken);
        return Ok(stations);
    }
}
