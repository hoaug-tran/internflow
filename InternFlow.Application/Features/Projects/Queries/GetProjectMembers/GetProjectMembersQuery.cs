using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectMembers;

public sealed record GetProjectMembersQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<ProjectMemberResponse>>;
