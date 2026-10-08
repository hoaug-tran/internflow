using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluationById;

public record GetMyEvaluationByIdQuery(Guid Id) : IRequest<ApiResponse<EvaluationDto>>;
