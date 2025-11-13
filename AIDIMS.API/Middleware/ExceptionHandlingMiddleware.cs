using AIDIMS.Application.Common;
using AIDIMS.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace AIDIMS.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message, Array.Empty<string>()),

            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, "Validation failed",
                    validationEx.Errors.SelectMany(e => e.Value).ToArray()),

            DomainException domainEx =>
                (HttpStatusCode.BadRequest, domainEx.Message, Array.Empty<string>()),

            _ =>
                (HttpStatusCode.InternalServerError, "An internal server error occurred",
                    Array.Empty<string>())
        };

        context.Response.StatusCode = (int)statusCode;

        var result = Result.Failure(message, errors);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, jsonOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
