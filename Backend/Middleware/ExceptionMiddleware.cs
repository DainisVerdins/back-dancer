using Application.Entities.Common;
using FluentValidation;
using MediatR;
using System.Net;
using System.Text.Json;

namespace WebApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
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
        context.Response.ContentType = "application/json";

        BaseResponse<Unit> response;

        switch (exception)
        {
            case ValidationException validationEx:
                var errorMessages = validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();
                _logger.LogWarning(validationEx, "Validation failed: {Errors}", string.Join(", ", errorMessages));
                response = new BaseResponse<Unit>(Unit.Value, errorMessages, HttpStatusCode.BadRequest);
                break;

            case UnauthorizedAccessException:
                _logger.LogWarning(exception, "Unauthorized access attempt");
                response = new BaseResponse<Unit>("Unauthorized access.", HttpStatusCode.Unauthorized);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred");
                response = new BaseResponse<Unit>("An internal server error occurred.", HttpStatusCode.InternalServerError);
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
