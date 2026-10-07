using FluentValidation;
using InternFlow.Shared.Constants;

namespace InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogs;

public sealed class GetWorkLogsQueryValidator
    : AbstractValidator<GetWorkLogsQuery>
{
    public GetWorkLogsQueryValidator()
    {
        RuleFor(x => x.Parameters.SortDir)
            .Must(SortDirections.IsValid)
            .WithMessage("sortDir chỉ nhận 'asc' hoặc 'desc'");

        RuleFor(x => x.Parameters)
            .Must(p => !p.From.HasValue || !p.To.HasValue || p.To >= p.From)
            .WithMessage("To phải sau From.");
    }
}
