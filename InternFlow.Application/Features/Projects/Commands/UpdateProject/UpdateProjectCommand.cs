using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    UpdateProjectRequest Request)
    : IRequest<ProjectResponse>;
