using AutoMapper;
using Domain.Entities;
using Persistence.Identity.Models;

namespace Infrastructure.MappingProfiles;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<AppUser, User>()
            .ReverseMap();
        CreateMap<AppRole, Role>()
           .ReverseMap();
    }
}
