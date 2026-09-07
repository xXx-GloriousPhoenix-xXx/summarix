using Microsoft.AspNetCore.Mvc;
using Summarix.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Summarix.Presentation.Exceptions;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request was canceled by the client.");
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, GetOptions());
        }
    }

    private static JsonSerializerOptions GetOptions()
    {
        return new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, JsonSerializerOptions options)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title) = exception switch
        {
            InvalidVideoSourceException => (HttpStatusCode.BadRequest, "Invalid Video Source"),
            TranscriptionFailedException => (HttpStatusCode.BadGateway, "Transcription Service Failed"),
            SummarizationFailedException => (HttpStatusCode.BadGateway, "Summarization Service Failed"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected server error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };
        var json = JsonSerializer.Serialize(problemDetails, options);

        return context.Response.WriteAsync(json);
    }
}
