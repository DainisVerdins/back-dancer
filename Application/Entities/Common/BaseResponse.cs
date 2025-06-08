using System.Net;

namespace Application.Entities.Common;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;

    public List<string> ErrorMessages { get; set; } = [];

    public BaseResponse(T? data)
    {
        Data = data;
        StatusCode = HttpStatusCode.OK;
    }
    public BaseResponse(T? data, HttpStatusCode statusCode) : this(data)
    {
        StatusCode = statusCode;
    }

    public BaseResponse(string errorMessage, HttpStatusCode statusCode)
    {
        Data = default(T);
        IsSuccess = false;
        StatusCode = statusCode;

        if (!string.IsNullOrEmpty(errorMessage))
            ErrorMessages = new List<string>() { errorMessage };
    }

    public BaseResponse(T? data, IEnumerable<string> errorMessages, HttpStatusCode statusCode) : this(data, statusCode)
    {
        IsSuccess = false;

        var errorsToAdd = new List<string>();
        if (errorMessages != null)
            errorsToAdd = [.. errorMessages];

        ErrorMessages = errorsToAdd;
    }
    public BaseResponse(T? data, string errorMessage, HttpStatusCode statusCode)
    {
        Data = data;
        IsSuccess = false;
        StatusCode = statusCode;

        if (!string.IsNullOrEmpty(errorMessage))
            ErrorMessages = new List<string>() { errorMessage };
    }
}
