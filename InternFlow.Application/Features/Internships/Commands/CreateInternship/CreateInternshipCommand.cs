using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Internships.Commands.CreateInternship;

public record CreateInternshipCommand(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid CreatedBy
) : IRequest<ApiResponse<InternshipDto>>;
