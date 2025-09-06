using Microsoft.EntityFrameworkCore;
using ProductCatalogAPI.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace ProductCatalogAPI.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response;

        switch (exception)
        {
            case InsufficientStockException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { error = exception.Message, type = exception.GetType().Name };
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new { error = exception.Message, type = exception.GetType().Name };
                break;
            case DbUpdateConcurrencyException:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response = new { error = "The record was modified by another user. Please refresh and try again.", type = exception.GetType().Name };
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new { error = "An internal server error occurred.", type = "InternalServerError" };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}