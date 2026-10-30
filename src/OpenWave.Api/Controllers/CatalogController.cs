using Microsoft.AspNetCore.Mvc;
using OpenWave.Api.Services;
using OpenWave.Contracts;

namespace OpenWave.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(
    ICatalogService catalogService,
    ILogger<CatalogController> logger) : ControllerBase
{
    private const string CatalogueUnavailableDetail = "No se pudo obtener el catálogo de emisoras.";

    /// <summary>
    /// Countries with working stations, sorted by name.
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(IReadOnlyList<OptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public Task<ActionResult<IReadOnlyList<OptionDto>>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return LoadAsync(catalogService.GetCountriesAsync, cancellationToken);
    }

    /// <summary>
    /// The 100 languages with the most stations, sorted by label.
    /// </summary>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(IReadOnlyList<OptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public Task<ActionResult<IReadOnlyList<OptionDto>>> GetLanguagesAsync(CancellationToken cancellationToken)
    {
        return LoadAsync(catalogService.GetLanguagesAsync, cancellationToken);
    }

    /// <summary>
    /// The 100 genres (tags) with the most stations, sorted by label.
    /// </summary>
    [HttpGet("genres")]
    [ProducesResponseType(typeof(IReadOnlyList<OptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public Task<ActionResult<IReadOnlyList<OptionDto>>> GetGenresAsync(CancellationToken cancellationToken)
    {
        return LoadAsync(catalogService.GetGenresAsync, cancellationToken);
    }

    /// <summary>
    /// The four fixed content types.
    /// </summary>
    [HttpGet("content-types")]
    [ProducesResponseType(typeof(IReadOnlyList<OptionDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OptionDto>> GetContentTypes()
    {
        return Ok(catalogService.GetContentTypes());
    }

    private async Task<ActionResult<IReadOnlyList<OptionDto>>> LoadAsync(
        Func<CancellationToken, Task<IReadOnlyList<OptionDto>>> load,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await load(cancellationToken));
        }
        catch (RadioBrowserUnavailableException ex)
        {
            logger.LogError("Catalogue options unavailable: {Reason}", ex.InnerException?.Message ?? ex.Message);
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Bad Gateway",
                detail: CatalogueUnavailableDetail);
        }
    }
}
