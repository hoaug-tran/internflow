using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Commands.AddProjectMember;

public sealed record AddProjectMemberCommand(
    Guid ProjectId,
    AddProjectMemberRequest Request)
    : IRequest<ProjectMemberResponse>;
