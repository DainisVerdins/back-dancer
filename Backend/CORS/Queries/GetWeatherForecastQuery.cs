using Backend.Models;
using MediatR;

namespace Backend.CORS.Queries;

public class GetWeatherForecastQuery : IRequest< BaseResponse<List<WeatherForecast>>>
{
    public int MaxNumberOfForecastToReturn { get; init; }
}
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, BaseResponse<List<WeatherForecast>>>
{
    private static readonly string[] Summaries = new[] {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    public GetWeatherForecastQueryHandler()
    {
    }

    public async Task<BaseResponse<List<WeatherForecast>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
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
        var output = await resultTask;

        return new BaseResponse<List<WeatherForecast>>(output, System.Net.HttpStatusCode.OK);
    }
}