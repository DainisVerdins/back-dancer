using Backend.Models;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace Backend.Middleware;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _enviroment;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _enviroment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = _enviroment.IsDevelopment()
               ? new CustomExceptionResponse(HttpStatusCode.InternalServerError, exception.Message, exception?.StackTrace?.ToString())
               : new CustomExceptionResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
