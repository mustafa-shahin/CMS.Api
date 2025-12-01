using CMS.Application.Common.Exceptions;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace CMS.Api.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// Converts exceptions to appropriate HTTP responses with problem details.
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
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationProblemDetails(context, validationEx)),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateProblemDetails(context, HttpStatusCode.NotFound,
                    "Not Found", notFoundEx.Message, ErrorCodes.NotFound)),

            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                CreateProblemDetails(context, HttpStatusCode.Unauthorized,
                    "Unauthorized", unauthorizedEx.Message, unauthorizedEx.ErrorCode)),

            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                CreateProblemDetails(context, HttpStatusCode.Forbidden,
                    "Forbidden", forbiddenEx.Message, forbiddenEx.ErrorCode)),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                CreateProblemDetails(context, HttpStatusCode.UnprocessableEntity,
                    "Business Rule Violation", businessEx.Message, businessEx.ErrorCode)),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(context, HttpStatusCode.InternalServerError,
                    "Internal Server Error", GetErrorMessage(exception), ErrorCodes.InternalError))
        };

        // Log the exception
        LogException(exception, statusCode);

        // Write response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail,
        string errorCode)
    {
        return new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["errorCode"] = errorCode,
                ["traceId"] = context.TraceIdentifier
            }
        };
    }

    private ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception)
    {
        return new ValidationProblemDetails(exception.Errors)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path,
            Extensions =
            {
                ["errorCode"] = ErrorCodes.ValidationFailed,
                ["traceId"] = context.TraceIdentifier
            }
        };
    }

    private string GetErrorMessage(Exception exception)
    {
        // Only expose detailed error messages in development
        return _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Information,
            _ => LogLevel.Warning
        };

        _logger.Log(logLevel, exception, "Exception occurred: {Message}", exception.Message);
    }
}