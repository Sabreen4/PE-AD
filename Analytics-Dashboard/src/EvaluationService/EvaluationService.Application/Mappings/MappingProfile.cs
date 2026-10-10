using AutoMapper;
using EvaluationService.Application.Dtos.Attendance;
using EvaluationService.Application.Dtos.Feedback;
using EvaluationService.Application.Dtos.Leaderboard;
using EvaluationService.Application.Dtos.WeeklyEvaluation;
using EvaluationService.Domain.Entities;

namespace EvaluationService.Application.Mappings;

/// <summary>
/// AutoMapper profile defining mappings between domain entities and DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Attendance, AttendanceDto>();
        CreateMap<WeeklyEvaluation, WeeklyEvaluationDto>();
        CreateMap<Leaderboard, LeaderboardEntryDto>();

        CreateMap<CreateAttendanceDto, Attendance>();

        CreateMap<Feedback, FeedbackDto>()
            .ForMember(d => d.FeedbackId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FeedbackComment, o => o.MapFrom(s => s.Comment))
            .ForMember(d => d.FeedbackTypeCode, o => o.MapFrom(s => s.FeedbackTypeCode))
            .ForMember(d => d.FeedbackTypeName, o => o.MapFrom(s => s.FeedbackTypeName))
            .ForMember(d => d.FeedbackScoreAdjustment, o => o.MapFrom(s => s.FeedbackScoreAdjustment));
    }
}
