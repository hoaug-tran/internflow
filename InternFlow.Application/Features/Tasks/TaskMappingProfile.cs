using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Projects;

namespace InternFlow.Application.Features.Tasks;

public sealed class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskResponse>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.AssigneeName,
                o => o.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null));

        // Request -> Entity (Create)
        CreateMap<CreateTaskRequest, TaskItem>()
            .IgnoreAuditAndNavigation()
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CompletedAt, o => o.Ignore())
            .ForMember(d => d.Project, o => o.Ignore())
            .ForMember(d => d.Assignee, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.WorkLogs, o => o.Ignore());

        CreateMap<UpdateTaskRequest, TaskItem>()
            .IgnoreAuditAndNavigation()
            .ForMember(d => d.ProjectId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CompletedAt, o => o.Ignore())
            .ForMember(d => d.Project, o => o.Ignore())
            .ForMember(d => d.Assignee, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.WorkLogs, o => o.Ignore());
    }
}
