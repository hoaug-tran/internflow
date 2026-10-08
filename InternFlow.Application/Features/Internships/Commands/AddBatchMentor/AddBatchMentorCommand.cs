using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchMentor;

public record AddBatchMentorCommand(
    Guid InternshipId,
    Guid MentorId,
    Guid? AddedBy = null
) : IRequest<ApiResponse<bool>>;
