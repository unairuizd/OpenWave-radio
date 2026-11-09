using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OpenWave.Api.Services;

namespace OpenWave.Api.Controllers;

/// <summary>
/// Turns a failed <see cref="ServiceResult{T}"/> into a ProblemDetails response. The result's message is a key of
/// Resources/Messages.resx, answered in the request language.
/// </summary>
public static class ServiceResultExtensions
{
    public static ActionResult ToProblem<T>(this ControllerBase controller, ServiceResult<T> result)
    {
        var localizer = controller.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();
        var detail = localizer[result.Message!].Value;

        if (result.Error == ServiceError.Invalid)
        {
            var errors = new ModelStateDictionary();
            errors.AddModelError(result.Field ?? string.Empty, detail);
            return controller.ValidationProblem(
                detail: detail,
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

        return controller.Problem(statusCode: statusCode, title: title, detail: detail);
    }
}
