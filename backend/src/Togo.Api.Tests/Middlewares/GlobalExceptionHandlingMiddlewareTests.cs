using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Togo.Api.Middlewares;

namespace Togo.Api.Tests.Middlewares;

public sealed class GlobalExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenUnhandledExceptionOccurs_ReturnsSafeInternalServerErrorResponse()
    {
        // Arrange
        var previousActivity = Activity.Current;
        Activity.Current = null;

        try
        {
            var context = new DefaultHttpContext
            {
                TraceIdentifier = "test-trace-id"
            };
            context.Response.Body = new MemoryStream();

            var exception = new InvalidOperationException("Sensitive database failure with password=123");
            RequestDelegate next = _ => throw exception;
            var logger = new TestLogger<GlobalExceptionHandlingMiddleware>();
            var middleware = new GlobalExceptionHandlingMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var responseBody = await ReadResponseBodyAsync(context.Response.Body);

            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.StartsWith("application/json", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Sensitive database failure", responseBody, StringComparison.Ordinal);
            Assert.DoesNotContain("password=123", responseBody, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(InvalidOperationException), responseBody, StringComparison.Ordinal);
            Assert.DoesNotContain("StackTrace", responseBody, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty("message", out var message));
            Assert.Equal("An unexpected error occurred.", message.GetString());

            Assert.True(root.TryGetProperty("traceId", out var traceId));
            Assert.Equal("test-trace-id", traceId.GetString());
            Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));

            var errorEntry = Assert.Single(logger.Entries, entry => entry.Level == LogLevel.Error);
            Assert.NotNull(errorEntry.Exception);
            Assert.Same(exception, errorEntry.Exception);
        }
        finally
        {
            Activity.Current = previousActivity;
        }
    }

    [Fact]
    public async Task InvokeAsync_WhenNoExceptionOccurs_DoesNotChangeSuccessfulResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        };
        var logger = new TestLogger<GlobalExceptionHandlingMiddleware>();
        var middleware = new GlobalExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    private static async Task<string> ReadResponseBodyAsync(Stream responseBody)
    {
        responseBody.Position = 0;

        using var reader = new StreamReader(responseBody, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<TestLogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new TestLogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    private sealed record TestLogEntry(LogLevel Level, string Message, Exception? Exception);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
