using System.Net;
using System.Text.Json;
using Engrslan.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Engrslan.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(context, exception);
        
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => new ValidationProblemDetails(validationException.Errors)
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Error",
                Detail = validationException.Message,
                Instance = context.Request.Path
            },

            NotFoundException notFoundException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Not Found",
                Detail = notFoundException.Message,
                Instance = context.Request.Path
            },

            UnauthorizedException unauthorizedException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = unauthorizedException.Message,
                Instance = context.Request.Path
            },

            ForbiddenException forbiddenException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Forbidden,
                Title = "Forbidden",
                Detail = forbiddenException.Message,
                Instance = context.Request.Path
            },

            ConflictException conflictException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Conflict",
                Detail = conflictException.Message,
                Instance = context.Request.Path
            },

            DomainException domainException => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Business Rule Violation",
                Detail = domainException.Message,
                Instance = context.Request.Path
            },

            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request.",
                Instance = context.Request.Path
            }
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }
}