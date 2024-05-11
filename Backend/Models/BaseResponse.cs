using System.Net;

namespace Backend.Models;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public bool HasError { get; set; } = false;
    public string? ErrorMessage { get; set; } = null!;
    public string? Details { get; set; } = null!;
    public HttpStatusCode StatusCode { get; set; }

    public BaseResponse(T? data, bool hasError, string? errorMessage, HttpStatusCode statusCode)
    {
        Data = data;
        HasError = hasError;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public BaseResponse(T? data, HttpStatusCode statusCode)
    {
        Data = data;
        HasError = false;
        StatusCode = statusCode;
    }

    public BaseResponse(HttpStatusCode statusCode, string? errorMessage, string? errorMessageDetails)
    {
        Data = default;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
        Details = errorMessageDetails;
    }
    public BaseResponse(Task<List<WeatherForecast>> resultTask)
    {
    }
}
