using Application.Entities;
using AutoMapper;
using Domain.Models;

namespace Application.MappingProfiles;

public class TokenMappingProfile : Profile
{
    public TokenMappingProfile()
    {
        CreateMap<TokenResponse, RefreshToken>();
    }
}
