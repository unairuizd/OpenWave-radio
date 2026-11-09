using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;
using MySql.Data.MySqlClient;

namespace OpenWave.Api.Controllers;

/// <summary>
/// Turns any MySQL failure that reaches a controller into 503 with a message in the request language.
/// </summary>
public sealed class DatabaseExceptionFilter(
    ProblemDetailsFactory problemDetailsFactory,
    IStringLocalizer<Messages> localizer,
    ILogger<DatabaseExceptionFilter> logger) : IExceptionFilter
{
    private const int AccessDeniedErrorNumber = 1045;

    public void OnException(ExceptionContext context)
    {
        var mySqlException = FindMySqlException(context.Exception);
        if (mySqlException is null)
        {
            return;
        }

        // Only the exception type and message are logged: never the request body or the connection string.
        // The access-denied message contains the database user name, so it is replaced.
        var reason = mySqlException.Number == AccessDeniedErrorNumber
            ? "Access denied: check the user and password stored in user-secrets."
            : mySqlException.Message;
        logger.LogError("Database unavailable: {ExceptionType}: {Reason}", context.Exception.GetType().Name, reason);

        var problem = problemDetailsFactory.CreateProblemDetails(
            context.HttpContext,
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service Unavailable",
            detail: localizer["DatabaseUnavailable"].Value);
        context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        context.ExceptionHandled = true;
    }

    private static MySqlException? FindMySqlException(Exception? exception)
    {
        // EF Core may wrap the provider exception (DbUpdateException, InvalidOperationException).
        while (exception is not null)
        {
            if (exception is MySqlException mySqlException)
            {
                return mySqlException;
            }

            exception = exception.InnerException;
        }

        return null;
    }
}
