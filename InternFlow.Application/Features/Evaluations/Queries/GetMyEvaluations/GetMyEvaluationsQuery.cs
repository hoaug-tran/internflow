using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluations;

public record GetMyEvaluationsQuery : IRequest<ApiResponse<List<MyEvaluationSummaryDto>>>;
