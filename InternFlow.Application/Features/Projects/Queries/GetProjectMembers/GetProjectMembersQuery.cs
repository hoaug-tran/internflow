using InternFlow.Application.Common.Caching;
using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectMembers;

[Cacheable("project:members", ttlMinutes: 10)]
public sealed record GetProjectMembersQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<ProjectMemberResponse>>;
