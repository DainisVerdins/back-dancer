using AutoMapper;
using Domain.Models;
using WebApi.Models.Dtos;
namespace WebApi.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();
    }
}
