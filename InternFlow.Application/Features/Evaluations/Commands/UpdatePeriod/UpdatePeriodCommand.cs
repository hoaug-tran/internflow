using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdatePeriod;

public record UpdatePeriodCommand(Guid Id, string Name, string? Description, DateOnly StartDate, DateOnly EndDate)
    : IRequest<ApiResponse<EvaluationPeriodDto>>;
