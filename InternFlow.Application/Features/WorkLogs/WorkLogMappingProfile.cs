using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Projects;

namespace InternFlow.Application.Features.WorkLogs;

public sealed class WorkLogMappingProfile : Profile
{
    public WorkLogMappingProfile()
    {
        CreateMap<WorkLog, WorkLogResponse>()
            .ForMember(d => d.TaskTitle, o => o.MapFrom(s => s.TaskItem.Title))
            .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.TaskItem.ProjectId))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));

        CreateMap<CreateWorkLogRequest, WorkLog>()
            .IgnoreAuditAndNavigation()
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore())
            .ForMember(d => d.TaskItem, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());
    }
}
