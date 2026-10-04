using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Projects;

namespace InternFlow.Application.Features.Projects;

public sealed class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        // 1. Entity → Response
        CreateMap<Project, ProjectResponse>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // 2. Create Request → Entity
        CreateMap<CreateProjectRequest, Project>()
            .IgnoreAuditAndNavigation()
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Internship, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.Members, o => o.Ignore())
            .ForMember(d => d.Tasks, o => o.Ignore());

        // 3. Update Request → Entity
        CreateMap<UpdateProjectRequest, Project>()
            .IgnoreAuditAndNavigation()
            .ForMember(d => d.InternshipId, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.Internship, o => o.Ignore())
            .ForMember(d => d.Creator, o => o.Ignore())
            .ForMember(d => d.Members, o => o.Ignore())
            .ForMember(d => d.Tasks, o => o.Ignore());

        CreateMap<ProjectMember, ProjectMemberResponse>()
            .ForCtorParam("FullName", o => o.MapFrom(s => s.User.FullName))
            .ForCtorParam("Email", o => o.MapFrom(s => s.User.Email))
            .ForCtorParam("Role", o => o.MapFrom(s => s.Role.ToString()));


    }
}
