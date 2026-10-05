using FluentValidation;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Shared.Constants;

namespace InternFlow.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        RuleFor(x => x.Parameters.SortBy)
            .Must(TaskSortFields.IsAllowed)
            .WithMessage($"sortBy chỉ nhận: {string.Join(", ", TaskSortFields.Allowed)}");

        RuleFor(x => x.Parameters.SortDir)
            .Must(SortDirections.IsValid)
            .WithMessage("sortDir chỉ nhận 'asc' hoặc 'desc'");

        RuleFor(x => x.Parameters.Keyword)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Parameters.Keyword));
    }
}
