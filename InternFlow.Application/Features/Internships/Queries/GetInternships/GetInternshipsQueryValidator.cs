using FluentValidation;

namespace InternFlow.Application.Features.Internships.Queries.GetInternships;

public sealed class GetInternshipsQueryValidator : AbstractValidator<GetInternshipsQuery>
{
    public GetInternshipsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page phải >= 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Trạng thái đợt thực tập không hợp lệ.");
    }
}
