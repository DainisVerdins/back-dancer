using AutoMapper;
using Backend.Models;
using Backend.Models.Dtos;
using MediatR;

namespace Backend.CORS.Queries;

public class GetWeatherForecastQuery : IRequest<BaseResponse<List<WeatherForecastDto>>>
{
    public int MaxNumberOfForecastToReturn { get; init; }
}
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, BaseResponse<List<WeatherForecastDto>>>
{
    private static readonly string[] Summaries = new[] {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    private readonly IMapper _mapper;
    public GetWeatherForecastQueryHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<WeatherForecastDto>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        var resultTask = Task<List<WeatherForecast>>.Factory.StartNew(() =>
        {
            var generatedForeCasts = new List<WeatherForecast>();
            for (int i = 0; i < request.MaxNumberOfForecastToReturn; i++)
            {
                generatedForeCasts.Add(new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                });
            }

            return generatedForeCasts;
        });
        var output = (await resultTask).Select(_mapper.Map<WeatherForecastDto>).ToList();

        return new BaseResponse<List<WeatherForecastDto>>(output, System.Net.HttpStatusCode.OK);
    }
}