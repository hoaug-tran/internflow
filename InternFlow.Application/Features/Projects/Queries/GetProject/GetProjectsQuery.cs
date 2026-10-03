using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Shared.Models;
using MediatR;

namespace InternFlow.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(ProjectQueryParameters Parameters)
    : IRequest<PagedResult<ProjectListItemResponse>>;