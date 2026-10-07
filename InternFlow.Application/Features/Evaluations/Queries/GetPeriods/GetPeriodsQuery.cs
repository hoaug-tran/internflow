using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriods;

public record GetPeriodsQuery(Guid? InternshipId) : IRequest<ApiResponse<List<EvaluationPeriodDto>>>;
