using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateCriterion;

public record UpdateCriterionCommand(Guid PeriodId, Guid Id, string Name, string? Description, decimal Weight, int DisplayOrder, bool IsActive)
    : IRequest<ApiResponse<CriterionDto>>;
