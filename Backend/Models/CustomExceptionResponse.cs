using System.Net;

namespace Backend.Models;

public class CustomExceptionResponse
{
    public string ErrorMessage { get; set; } = null!;
    public string? Details { get; set; } = null!;
    public HttpStatusCode StatusCode { get; set; }

    public CustomExceptionResponse(HttpStatusCode statusCode, string errorMessage, string? errorMessageDetails)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
        Details = errorMessageDetails;
    }
    public CustomExceptionResponse(HttpStatusCode statusCode, string errorMessage)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}
