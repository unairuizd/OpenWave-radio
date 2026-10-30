using Microsoft.AspNetCore.Mvc;
using OpenWave.Api.Services;
using OpenWave.Contracts;

namespace OpenWave.Api.Controllers;

/// <remarks>
/// Known limitation of this course project: these endpoints are not authorised; the session lives only in the app.
/// </remarks>
[ApiController]
[Route("api/users/{userId:long}/favorites")]
public sealed class FavoritesController(IFavoriteService favoriteService) : ControllerBase
{
    /// <summary>
    /// Returns the user's favourite stations, newest first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<StationDto>>> GetAsync(long userId, CancellationToken cancellationToken)
    {
        var result = await favoriteService.ListAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result);
    }

    /// <summary>
    /// Saves a copy of the station as a favourite. Values longer than the columns are shortened.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<StationDto>> AddAsync(long userId, StationDto station, CancellationToken cancellationToken)
    {
        var result = await favoriteService.AddAsync(userId, station, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToProblem(result);
    }

    /// <summary>
    /// Removes a station from the user's favourites.
    /// </summary>
    [HttpDelete("{stationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RemoveAsync(long userId, string stationId, CancellationToken cancellationToken)
    {
        var result = await favoriteService.RemoveAsync(userId, stationId, cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToProblem(result);
    }
}
