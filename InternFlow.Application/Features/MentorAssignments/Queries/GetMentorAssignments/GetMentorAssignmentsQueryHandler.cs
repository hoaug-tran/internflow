using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.MentorAssignments.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.MentorAssignments.Queries.GetMentorAssignments;

public class GetMentorAssignmentsQueryHandler : IRequestHandler<GetMentorAssignmentsQuery, ApiResponse<List<MentorAssignmentDto>>>
{
    private readonly IInternFlowDbContext _context;

    public GetMentorAssignmentsQueryHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<MentorAssignmentDto>>> Handle(GetMentorAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MentorAssignments
            .AsNoTracking()
            .Where(ma => ma.InternshipId == request.InternshipId);

        if (request.MentorId.HasValue)
        {
            query = query.Where(ma => ma.MentorId == request.MentorId.Value);
        }

        if (request.InternId.HasValue)
        {
            query = query.Where(ma => ma.InternId == request.InternId.Value);
        }

        if (request.OnlyCurrent)
        {
            query = query.Where(ma => ma.EndedAt == null);
        }

        var items = await query
            .OrderByDescending(ma => ma.AssignedAt)
            .Select(ma => new MentorAssignmentDto
            {
                Id = ma.Id,
                InternshipId = ma.InternshipId,
                InternshipName = ma.Internship.Name,
                InternId = ma.InternId,
                InternName = ma.BatchIntern.Intern.FullName,
                InternEmail = ma.BatchIntern.Intern.Email,
                MentorId = ma.MentorId,
                MentorName = ma.BatchMentor.Mentor.FullName,
                MentorEmail = ma.BatchMentor.Mentor.Email,
                AssignedAt = ma.AssignedAt,
                AssignedBy = ma.AssignedBy,
                EndedAt = ma.EndedAt,
                EndReason = ma.EndReason,
                IsCurrent = ma.EndedAt == null
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<MentorAssignmentDto>>.Ok(items);
    }
}
