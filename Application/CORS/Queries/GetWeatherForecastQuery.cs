using Application.Entities.Common;
using Application.Entities.Dtos;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.CORS.Queries;

public class GetWeatherForecastQuery : IRequest<BaseResponse<IEnumerable<WeatherForecastDto>>>
{
    public int MaxNumberOfForecastToReturn { get; init; }
}
public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, BaseResponse<IEnumerable<WeatherForecastDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public GetWeatherForecastQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public Task<BaseResponse<IEnumerable<WeatherForecastDto>>> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        var weatherForeCasts = _unitOfWork.WeatherForecasts.GetPopularDevelopers(request.MaxNumberOfForecastToReturn);
        var output = weatherForeCasts.Select(w => _mapper.Map<WeatherForecastDto>(w)).ToList();


        return Task.FromResult(new BaseResponse<IEnumerable<WeatherForecastDto>>(output, System.Net.HttpStatusCode.OK));
    }
}