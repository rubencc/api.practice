using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Weather.Host.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (
                (int)HttpStatusCode.UnprocessableEntity,  // 422
                "Validation Error"
            ),
            NotFoundException => (
                (int)HttpStatusCode.NotFound,
                "Resource Not Found"
            ),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Operation"
            ),
            ArgumentNullException => (
                (int)HttpStatusCode.BadRequest,
                "Argument Null"
            ),
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                "Invalid Argument"
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error"
            )
        };

        var detail = exception switch
        {
            ArgumentNullException argNullEx => $"Required argument '{argNullEx.ParamName}' is null",
            _ when statusCode == (int)HttpStatusCode.InternalServerError => 
                "An unexpected error occurred. Please try again later.",
            _ => exception.Message
        };

        // Obtener TraceId de Activity.Current (OpenTelemetry)
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId
            }
        };
    }
}

