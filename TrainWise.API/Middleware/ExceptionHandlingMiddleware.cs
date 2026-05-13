using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace TrainWise.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("File exceeds", StringComparison.OrdinalIgnoreCase) ||
                                                     ex.Message.Contains("size", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "File size limit exceeded");
            await WriteErrorAsync(context, HttpStatusCode.RequestEntityTooLarge, "File too large", ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Only CSV", StringComparison.OrdinalIgnoreCase) ||
                                                     ex.Message.Contains("Unsupported file", StringComparison.OrdinalIgnoreCase) ||
                                                     ex.Message.Contains("format", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Invalid file format");
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Invalid file format", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error");
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Validation error", ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ML Service communication error");
            await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                "ML Service unavailable",
                "The ML training service is not reachable. Please ensure it is running on the configured port.");
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Request timeout");
            await WriteErrorAsync(context, HttpStatusCode.GatewayTimeout,
                "Request timeout",
                "The operation timed out. For large datasets, try reducing the number of rows.");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database connectivity error");
            await WriteErrorAsync(context, HttpStatusCode.ServiceUnavailable,
                "Database unavailable",
                "The database is not reachable. Please verify SQL Server instance/service, connection string, and network settings.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                "Unexpected error",
                "An unexpected error occurred. Please try again.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
