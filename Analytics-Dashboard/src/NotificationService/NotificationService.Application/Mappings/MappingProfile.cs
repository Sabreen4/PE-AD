using AutoMapper;
using NotificationService.Application.Dtos.Notifications;
using NotificationService.Application.Features.Notifications.Commands;

namespace NotificationService.Application.Mappings;

/// <summary>
/// AutoMapper profile defining mappings between request DTOs and MediatR commands.
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>
    /// Initializes the mapping configuration.
    /// </summary>
    public MappingProfile()
    {
        CreateMap<SendRegistrationEmailRequestDto, SendRegistrationEmailCommand>();
        CreateMap<SendOtpEmailRequestDto, SendOtpEmailCommand>();
    }
}
