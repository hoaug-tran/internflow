using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetCriteria;

public record GetCriteriaQuery(Guid PeriodId) : IRequest<ApiResponse<List<CriterionDto>>>;
