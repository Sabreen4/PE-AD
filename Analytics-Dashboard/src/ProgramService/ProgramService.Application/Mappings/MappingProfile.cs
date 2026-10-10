using AutoMapper;
using ProgramService.Application.Dtos.Assignments;
using ProgramService.Application.Dtos.EmployeeAssignments;
using ProgramService.Application.Dtos.Enrollments;
using ProgramService.Application.Dtos.Programs;
using ProgramService.Application.Dtos.ProgramWeeks;
using ProgramService.Application.Dtos.Videos;
using ProgramService.Domain.Entities;

namespace ProgramService.Application.Mappings;

/// <summary>
/// AutoMapper profile defining mappings between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TrainingPrograms, ProgramDto>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<TrainingPrograms, ProgramDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<TrainingPrograms, ProgramCreatedDto>();

        CreateMap<ProgramWeek, ProgramWeekDto>();
        CreateMap<ProgramWeek, ProgramWeekDetailDto>()
            .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.TrainingProgramId));
        CreateMap<ProgramWeek, ProgramWeekCreatedDto>();
        CreateMap<ProgramWeek, VideoMetadataDto>();

        CreateMap<Assignment, AssignmentDto>()
            .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.TrainingProgramId))
            .ForMember(dest => dest.ProgramWeekId, opt => opt.MapFrom(src => src.ProgramWeekId))
            .ForMember(dest => dest.FileType, opt => opt.Ignore());
        CreateMap<Assignment, AssignmentDetailDto>()
            .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.TrainingProgramId))
            .ForMember(dest => dest.ProgramWeekId, opt => opt.MapFrom(src => src.ProgramWeekId))
            .ForMember(dest => dest.FileType, opt => opt.Ignore());
        CreateMap<Assignment, AssignmentCreatedDto>();

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.TrainingProgramId))
            .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<Enrollment, EnrollmentDetailDto>()
            .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.TrainingProgramId))
            .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<Enrollment, EnrollmentCreatedDto>();

        CreateMap<EmployeeAssignment, EmployeeAssignmentDto>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<EmployeeAssignment, EmployeeAssignmentDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<EmployeeAssignment, EmployeeAssignmentEnrichedDto>()
            .ForMember(dest => dest.EmployeeAssignmentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeName, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeEmail, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramId, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramName, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramWeekId, opt => opt.Ignore())
            .ForMember(dest => dest.ProgramWeekName, opt => opt.Ignore())
            .ForMember(dest => dest.AssignmentTitle, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.ReviewerComment))
            .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}

