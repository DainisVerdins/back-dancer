using Application.CORS.Animal;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles;

public class AnimalProfile : Profile
{
    public AnimalProfile()
    {
        CreateMap<CreateAnimalCommand, Animal>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());
    }
}
