using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateEvaluation;

public record UpdateEvaluationCommand(Guid Id, string? OverallComment, List<ScoreInput> Scores)
    : IRequest<ApiResponse<EvaluationDto>>;
