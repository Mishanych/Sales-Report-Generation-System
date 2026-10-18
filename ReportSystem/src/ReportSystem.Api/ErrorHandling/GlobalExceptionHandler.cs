using Microsoft.AspNetCore.Diagnostics;

namespace ReportSystem.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        await Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An unexpected error occurred",
                detail: "The server could not complete the request.",
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = httpContext.TraceIdentifier
                })
            .ExecuteAsync(httpContext);

        return true;
    }
}