using InternFlow.Application.Features.Projects.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(CreateProjectRequest Request)
    : IRequest<ProjectResponse>;
