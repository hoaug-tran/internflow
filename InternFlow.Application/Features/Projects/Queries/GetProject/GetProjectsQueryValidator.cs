using FluentValidation;
using InternFlow.Application.Features.Projects;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Constants;

namespace InternFlow.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
{
    public GetProjectsQueryValidator()
    {
        RuleFor(x => x.Parameters.SortBy)
            .Must(ProjectSortFields.IsAllowed)
            .WithMessage($"sortBy chỉ nhận: {string.Join(", ", ProjectSortFields.Allowed)}");

        RuleFor(x => x.Parameters.SortDir)
            .Must(SortDirections.IsValid)
            .WithMessage("sortDir chỉ nhận 'asc' hoặc 'desc'");

        RuleFor(x => x.Parameters.Status)
        .IsEnumName(typeof(ProjectStatus), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Parameters.Status))
            .WithMessage($"status chỉ nhận: {string.Join(", ", Enum.GetNames<ProjectStatus>())}");

        RuleFor(x => x.Parameters.Keyword)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Parameters.Keyword));
    }
}