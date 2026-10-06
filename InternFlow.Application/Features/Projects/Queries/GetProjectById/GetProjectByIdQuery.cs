using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid ProjectId)
    : IRequest<ProjectResponse>;
