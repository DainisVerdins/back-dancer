using AutoMapper;
using Backend.Models;
using Backend.Models.Dtos;

namespace Backend.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<WeatherForecast, WeatherForecastDto>();
    }
}
