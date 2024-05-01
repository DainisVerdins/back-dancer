using Backend.CORS.Queries;
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

    [MapToApiVersion("1.0")] // map each action to a specific version
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get([FromQuery] GetWeatherForecastQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather Forecast executing...");
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}