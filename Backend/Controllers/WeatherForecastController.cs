using Backend.CORS.Queries;
using Backend.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IMediator _mediator;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Endpoint what returns weather forecasts
    /// </summary>
    /// <remarks>
    /// All the parameters in the request body can be null. 
    ///
    ///  You can search by using any of the parameters in the request.
    ///  
    ///  NOTE: You can only search by one parameter at a time
    ///  
    /// Sample request:
    ///
    ///     POST /Get
    ///     {
    ///        "maxNumberOfForecastToReturn": 8
    ///     }
    /// </remarks>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>This endpoint returns a list of weather forecasts.</returns>
    [MapToApiVersion("1.0")] // map each action to a specific version
    [HttpGet(Name = "GetWeatherForecast")]
    [ProducesResponseType(typeof(BaseResponse<List<WeatherForecast>>), StatusCodes.Status200OK)]

    public async Task<IActionResult> Get([FromQuery] GetWeatherForecastQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather Forecast executing...");
        var response = await _mediator.Send(query, cancellationToken);

        return StatusCode((int)response.StatusCode, response);
    }
}