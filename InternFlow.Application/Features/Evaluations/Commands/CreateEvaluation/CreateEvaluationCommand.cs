using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateEvaluation;

public record CreateEvaluationCommand(Guid PeriodId, Guid InternId, string? OverallComment, List<ScoreInput> Scores)
    : IRequest<ApiResponse<EvaluationDto>>;
