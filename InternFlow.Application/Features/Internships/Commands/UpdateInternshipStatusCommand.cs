using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Commands.UpdateInternshipStatus;

public record UpdateInternshipStatusCommand(
    Guid InternshipId,
    InternshipStatus NewStatus
) : IRequest<ApiResponse<bool>>;
