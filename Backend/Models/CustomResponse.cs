using System.Net;

namespace Backend.Models;

public class CustomResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public CustomResponse(int statusCode, string message, string? details)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }

    public CustomResponse(int statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
}
