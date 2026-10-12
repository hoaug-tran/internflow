using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Commands.RestoreProjectMember;

public sealed record RestoreProjectMemberCommand(
    Guid ProjectId,
    Guid UserId)
    : IRequest<ProjectMemberResponse>;
