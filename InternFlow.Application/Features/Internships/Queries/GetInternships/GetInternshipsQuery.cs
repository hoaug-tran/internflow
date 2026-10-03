using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Queries.GetInternships;

public record GetInternshipsQuery(
    InternshipStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 10
) : IRequest<ApiResponse<List<InternshipDto>>>;
