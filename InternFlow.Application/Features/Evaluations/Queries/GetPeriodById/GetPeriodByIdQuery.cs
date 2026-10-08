using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriodById;

public record GetPeriodByIdQuery(Guid Id) : IRequest<ApiResponse<EvaluationPeriodDto>>;
