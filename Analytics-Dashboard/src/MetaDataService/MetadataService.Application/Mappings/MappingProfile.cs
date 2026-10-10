using AutoMapper;
using MetaDataService.Application.Dtos;
using MetaDataService.Domain.Entities;

namespace MetaDataService.Application.Mappings;

/// <summary>
/// AutoMapper profile defining mappings between domain entities and DTOs.
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class and configures all mappings.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<RefSet, RefSetResponseDto>()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Name));

        CreateMap<RefTerm, RefSetTermResponseDto>()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Description));

        CreateMap<SetRefTerm, RefSetTermResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RefTerm.Id))
            .ForMember(dest => dest.RefSetId, opt => opt.MapFrom(src => src.RefSetId))
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.RefTerm.Name))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.RefTerm.Description));
    }
}
