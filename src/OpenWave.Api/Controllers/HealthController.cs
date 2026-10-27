using Microsoft.AspNetCore.Mvc;

namespace OpenWave.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Reports whether the API is running.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse("ok", DateTime.UtcNow));
    }
}

public sealed record HealthResponse(string Status, DateTime TimeUtc);
