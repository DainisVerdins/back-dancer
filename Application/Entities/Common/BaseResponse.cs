using System.Net;

namespace Application.Entities.Common;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;

    public BaseResponse(T? data, HttpStatusCode statusCode)
    {
        Data = data;
        StatusCode = statusCode;
    }
}
