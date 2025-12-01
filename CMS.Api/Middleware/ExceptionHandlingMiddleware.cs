using System.Net;
using System.Text.Json;
using CMS.Application.Common.Exceptions;
using CMS.Application.Common.Models;
using CMS.Shared.Constants;
using FluentValidation;

namespace CMS.Api.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to standardized API responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        var (statusCode, errorCode, message, errors) = exception switch
        {
            Application.Common.Exceptions.ValidationException validationEx =>
                (HttpStatusCode.BadRequest, ErrorCodes.ValidationFailed, "One or more validation errors occurred.", validationEx.Errors),

            FluentValidation.ValidationException fluentValidationEx =>
                (HttpStatusCode.BadRequest, ErrorCodes.ValidationFailed, "One or more validation errors occurred.",
                    fluentValidationEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, ErrorCodes.NotFound, notFoundEx.Message, null),

            UnauthorizedException unauthorizedEx =>
                (HttpStatusCode.Unauthorized, unauthorizedEx.ErrorCode, unauthorizedEx.Message, null),

            ForbiddenAccessException forbiddenEx =>
                (HttpStatusCode.Forbidden, forbiddenEx.ErrorCode, forbiddenEx.Message, null),

            BusinessRuleException businessEx =>
                (HttpStatusCode.BadRequest, businessEx.ErrorCode, businessEx.Message, null),

            OperationCanceledException =>
                (HttpStatusCode.BadRequest, "REQUEST_CANCELLED", "The request was cancelled.", null),

            _ =>
                (HttpStatusCode.InternalServerError, ErrorCodes.InternalError, "An unexpected error occurred.", null)
        };

        // Log the exception
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("A handled exception occurred: {Message}", exception.Message);
        }

        // Build response
        var response = new ApiErrorResponse
        {
            Success = false,
            StatusCode = (int)statusCode,
            ErrorCode = errorCode,
            Message = message,
            Errors = errors,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        // Include stack trace in development
        if (_environment.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            response.Detail = exception.ToString();
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}