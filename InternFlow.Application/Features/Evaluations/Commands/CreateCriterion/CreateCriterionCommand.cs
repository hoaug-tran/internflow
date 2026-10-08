using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateCriterion;

public record CreateCriterionCommand(Guid PeriodId, string Name, string? Description, decimal Weight, int DisplayOrder)
    : IRequest<ApiResponse<CriterionDto>>;
