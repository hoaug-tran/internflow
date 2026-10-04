using FluentValidation;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectMembers;

public sealed class GetProjectMembersQueryValidator
    : AbstractValidator<GetProjectMembersQuery>
{
    public GetProjectMembersQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
