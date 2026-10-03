using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.MentorAssignments.Commands.EndMentorAssignment;

public record EndMentorAssignmentCommand(
    long AssignmentId,
    string? EndReason = null
) : IRequest<ApiResponse<bool>>;
