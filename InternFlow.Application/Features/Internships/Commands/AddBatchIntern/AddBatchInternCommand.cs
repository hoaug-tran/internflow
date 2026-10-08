using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchIntern;

public record AddBatchInternCommand(
    Guid InternshipId,
    Guid InternId,
    Guid? AddedBy = null,
    string? Notes = null
) : IRequest<ApiResponse<bool>>;
