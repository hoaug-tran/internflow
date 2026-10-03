using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;

public record AssignMentorCommand(
    Guid InternshipId,
    Guid InternId,
    Guid MentorId,
    Guid AssignedBy
) : IRequest<ApiResponse<long>>;
