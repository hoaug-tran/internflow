using MediatR;

namespace InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed record RemoveProjectMemberCommand(
    Guid ProjectId,
    Guid UserId)
    : IRequest<Unit>;
