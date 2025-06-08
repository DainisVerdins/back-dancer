using Application.Dtos;
using AutoMapper;
using Domain.Models;
namespace WebApi.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();
    }
}
