using Application.Constants;
using Application.Dtos;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => GetTitle(src.RoleCode)))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => GetDescription(src.RoleCode)))
            .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => GetIcon(src.RoleCode)))
            .ForMember(dest => dest.Color, opt => opt.MapFrom(src => GetColor(src.RoleCode)));
    }

    private static string GetTitle(string roleCode) => roleCode switch
    {
        UserRole.User => "I want to adopt",
        UserRole.ShelterWorker => "I am a Shelter Worker",
        UserRole.SuperAdmin => "I am master of this universe",
        _ => "Unknown Role"
    };

    private static string GetDescription(string roleCode) => roleCode switch
    {
        UserRole.User => "Find a new furry friend, save to favorites, and track your adoption requests.",
        UserRole.ShelterWorker => "Publish new pets, manage shelter stories, and process adoption applications.",
        UserRole.SuperAdmin => "Super user of the whole universe of PetPet",
        _ => "No description available."
    };

    private static string GetIcon(string roleCode) => roleCode switch
    {
        UserRole.User => "mdi-paw",
        UserRole.ShelterWorker => "mdi-home-heart",
        _ => "mdi-account-question"
    };

    private static string GetColor(string roleCode) => roleCode switch
    {
        UserRole.User => "primary",
        UserRole.ShelterWorker => "secondary",
        UserRole.SuperAdmin => "yellow",
        _ => "grey"
    };
}