using System.Net;

namespace WebApi.Models;

public class BaseResponse<T>
{
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public BaseResponse(T? data, HttpStatusCode statusCode)
    {
        Data = data;
        StatusCode = statusCode;
    }
}
