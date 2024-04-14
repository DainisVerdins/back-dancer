using Backend.Models;
using MediatR;
using System;
using System.Threading.Tasks;

namespace Backend.CORS.Queries;

public class GetWeatherForecastQuery : IRequest<List<WeatherForecast>>
{
    public int MaxNumberOfForecastToReturn { get; init; }
}
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, List<WeatherForecast>>
{
    private static readonly string[] Summaries = new[] {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public GetWeatherForecastQueryHandler()
    {
    }

    public Task<List<WeatherForecast>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {

        var resultTask = Task<List<WeatherForecast>>.Factory.StartNew(() =>
        {
            var output = new List<WeatherForecast>();
            for (int i = 0; i < request.MaxNumberOfForecastToReturn; i++)
            {
                output.Add(new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                });
            }

            return output;
        });

        return resultTask;
    }
}