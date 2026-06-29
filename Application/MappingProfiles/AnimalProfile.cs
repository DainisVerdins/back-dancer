using Application.ViewModels.Animal;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles;

public class AnimalProfile : Profile
{
    public AnimalProfile()
    {
        CreateMap<CreateAnimalViewModel, Animal>()
            .ForMember(dest => dest.Images, opt => opt.Ignore());
    }
}
