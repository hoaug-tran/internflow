using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.PublishEvaluation;

public record PublishEvaluationCommand(Guid Id) : IRequest<ApiResponse<EvaluationDto>>;
