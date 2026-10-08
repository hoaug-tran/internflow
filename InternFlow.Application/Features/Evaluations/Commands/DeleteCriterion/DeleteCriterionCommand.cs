using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.DeleteCriterion;

public record DeleteCriterionCommand(Guid PeriodId, Guid Id) : IRequest<ApiResponse<Guid>>;
