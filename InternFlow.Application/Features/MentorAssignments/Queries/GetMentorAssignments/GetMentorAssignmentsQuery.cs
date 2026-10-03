using InternFlow.Application.Features.MentorAssignments.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.MentorAssignments.Queries.GetMentorAssignments;

public record GetMentorAssignmentsQuery(
    Guid InternshipId,
    Guid? MentorId = null,
    Guid? InternId = null,
    bool OnlyCurrent = false
) : IRequest<ApiResponse<List<MentorAssignmentDto>>>;
