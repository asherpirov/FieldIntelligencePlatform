using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FieldIntelligenceApi.Middlewares;

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
        _logger.LogError(exception, "Unexpected error occurred");
        
        httpContext.Response.StatusCode = 500;
        
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred"
        }, cancellationToken);

        return true; 
    }
}