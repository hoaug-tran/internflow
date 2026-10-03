using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Queries.GetInternshipById;

public record GetInternshipByIdQuery(Guid Id) : IRequest<ApiResponse<InternshipDetailDto>>;
