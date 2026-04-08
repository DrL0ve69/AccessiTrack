using AccessiTrack.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace AccessiTrack.API.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error responses.
/// Maps different exception types to appropriate HTTP status codes.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.ErrorCode = "NOT_FOUND";
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = "VALIDATION_ERROR";
                // ValidationException.Errors is already grouped: Dictionary<string, string[]>
                response.Errors = validationEx.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.ErrorCode = "UNAUTHORIZED";
                break;

            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.ErrorCode = "INVALID_OPERATION";
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.ErrorCode = "INTERNAL_SERVER_ERROR";
                response.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Standard error response format for API errors.
/// </summary>
public class ErrorResponse
{
    public string ErrorCode { get; set; } = "UNKNOWN_ERROR";
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string[]>? Errors { get; set; }
}
