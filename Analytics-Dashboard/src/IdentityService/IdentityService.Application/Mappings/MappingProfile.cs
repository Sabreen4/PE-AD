using AutoMapper;
using IdentityService.Application.Dtos.Auth;
using IdentityService.Application.Dtos.Organizations;
using IdentityService.Application.Dtos.Users;
using IdentityService.Application.Features.Auth.Commands;
using IdentityService.Application.Features.Organizations.Commands;
using IdentityService.Application.Features.Organizations.Queries;
using IdentityService.Application.Features.Users.Commands;
using IdentityService.Application.Features.Users.Queries;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Mappings;

/// <summary>
/// Defines AutoMapper mappings between domain entities and DTOs for the Identity Service.
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingProfile"/> class and configures all mappings.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<Organization, OrganizationResponseDto>();
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<LoginRequestDto, LoginCommand>();
        CreateMap<VerifyOtpRequestDto, VerifyOtpCommand>();
        CreateMap<RefreshTokenRequestDto, RefreshTokenCommand>();
        CreateMap<LogoutRequestDto, LogoutCommand>();

        CreateMap<CreateOrganizationRequestDto, CreateOrganizationCommand>();
        CreateMap<CreateOrganizationRequestDto, UpdateOrganizationCommand>();

        CreateMap<CreateUserRequestDto, CreateUserCommand>();
        CreateMap<UpdateUserRequestDto, UpdateUserCommand>();
    }
}
