using System.Diagnostics;
using Togo.Api.Models;

namespace Togo.Api.Middlewares;

public sealed class GlobalExceptionHandlingMiddleware
{
    private const string UnexpectedErrorMessage = "An unexpected error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}",
                traceId);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    "The response has already started. The global exception middleware cannot handle the exception. TraceId: {TraceId}",
                    traceId);

                throw;
            }

            await HandleExceptionAsync(context, traceId);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string traceId)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse(UnexpectedErrorMessage, traceId);

        await context.Response.WriteAsJsonAsync(response);
    }
}
