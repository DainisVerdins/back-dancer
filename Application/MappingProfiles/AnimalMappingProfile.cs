using Application.ViewModels.Animal;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles;

public class AnimalMappingProfile : Profile
{
    public AnimalMappingProfile()
    {
        CreateMap<CreateAnimalViewModel, Animal>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));
    }
}
