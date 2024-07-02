using AutoMapper;
using Backend.Models.Dtos;
using Domain.Models;
namespace Backend.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();
    }
}
