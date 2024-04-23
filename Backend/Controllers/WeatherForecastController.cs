using Backend.CORS.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IMediator _mediator;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IActionResult> Get([FromQuery] GetWeatherForecastQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Weather Forecast executing...");
        var result = await _mediator.Send(query, cancellationToken);

       return Ok(result);
    }
}
