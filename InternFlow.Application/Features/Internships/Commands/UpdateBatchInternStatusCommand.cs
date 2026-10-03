using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Commands.UpdateBatchInternStatus;

public record UpdateBatchInternStatusCommand(
    Guid InternshipId,
    Guid InternId,
    BatchInternStatus NewStatus
) : IRequest<ApiResponse<bool>>;
