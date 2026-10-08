using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.CreatePeriod;

public record CreatePeriodCommand(Guid InternshipId, string Name, string? Description, DateOnly StartDate, DateOnly EndDate)
    : IRequest<ApiResponse<EvaluationPeriodDto>>;
