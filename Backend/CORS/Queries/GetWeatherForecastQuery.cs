using AutoMapper;
using Backend.Models;
using Backend.Models.Dtos;
using Domain.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetWeatherForecastQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<List<WeatherForecastDto>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        var weatherForeCasts = _unitOfWork.WeatherForecasts.GetPopularDevelopers(request.MaxNumberOfForecastToReturn);
        var output = weatherForeCasts.Select(w => _mapper.Map<WeatherForecastDto>(w)).ToList();


        return new BaseResponse<List<WeatherForecastDto>>(output, System.Net.HttpStatusCode.OK);
    }
}