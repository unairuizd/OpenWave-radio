using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OpenWave.Api.Services;

namespace OpenWave.Api.Controllers;

/// <summary>
/// Turns a failed <see cref="ServiceResult{T}"/> into a ProblemDetails response.
/// </summary>
public static class ServiceResultExtensions
{
    public static ActionResult ToProblem<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        if (result.Error == ServiceError.Invalid)
        {
            var errors = new ModelStateDictionary();
            errors.AddModelError(result.Field ?? string.Empty, result.Message!);
            return controller.ValidationProblem(
                detail: result.Message,
                title: "Bad Request",
                modelStateDictionary: errors);
        }

        var (statusCode, title) = result.Error switch
        {
            ServiceError.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ServiceError.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ServiceError.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => throw new InvalidOperationException($"Unexpected service error {result.Error}."),
        };

        return controller.Problem(statusCode: statusCode, title: title, detail: result.Message);
    }
}
