using AutoMapper;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Projects;

namespace InternFlow.Application.Common.Mappings;

public sealed class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        CreateMap<Project, ProjectResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Project, ProjectListItemResponse>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.MemberCount,
                opt => opt.MapFrom(s => s.Members.Count(m => m.RemovedAt == null)))
            .ForMember(d => d.TaskCount,
                opt => opt.MapFrom(s => s.Tasks.Count));

        CreateMap<CreateProjectRequest, Project>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore())
            .ForMember(d => d.Internship, opt => opt.Ignore())
            .ForMember(d => d.Creator, opt => opt.Ignore())
            .ForMember(d => d.Members, opt => opt.Ignore())
            .ForMember(d => d.Tasks, opt => opt.Ignore());
    }
}
