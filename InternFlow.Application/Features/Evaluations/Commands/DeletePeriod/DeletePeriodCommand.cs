using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.DeletePeriod;

public record DeletePeriodCommand(Guid Id) : IRequest<ApiResponse<Guid>>;
