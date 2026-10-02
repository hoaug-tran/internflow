using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Queries.GetInternshipById;

public record GetInternshipByIdQuery(Guid Id) : IRequest<ApiResponse<InternshipDetailDto>>;

public class GetInternshipByIdQueryHandler : IRequestHandler<GetInternshipByIdQuery, ApiResponse<InternshipDetailDto>>
{
    private readonly IInternFlowDbContext _context;

    public GetInternshipByIdQueryHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<InternshipDetailDto>> Handle(GetInternshipByIdQuery request, CancellationToken cancellationToken)
    {
        var internship = await _context.Internships
            .AsNoTracking()
            .Include(i => i.Creator)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (internship == null)
        {
            return ApiResponse<InternshipDetailDto>.Fail("Không tìm thấy đợt thực tập.");
        }

        // Lấy danh sách mentor trong đợt
        var mentors = await _context.BatchMentors
            .AsNoTracking()
            .Where(bm => bm.InternshipId == request.Id)
            .Include(bm => bm.Mentor)
                .ThenInclude(m => m.MentorProfile)
            .Select(bm => new BatchMentorDto
            {
                MentorId = bm.MentorId,
                FullName = bm.Mentor.FullName,
                Email = bm.Mentor.Email,
                Department = bm.Mentor.MentorProfile != null ? bm.Mentor.MentorProfile.Department : null,
                JobTitle = bm.Mentor.MentorProfile != null ? bm.Mentor.MentorProfile.JobTitle : null,
                MaxMentees = bm.Mentor.MentorProfile != null ? bm.Mentor.MentorProfile.MaxMentees : null,
                CurrentMenteeCount = _context.MentorAssignments
                    .Count(ma => ma.InternshipId == request.Id && ma.MentorId == bm.MentorId && ma.EndedAt == null),
                JoinedAt = bm.JoinedAt
            })
            .ToListAsync(cancellationToken);

        // Lấy danh sách intern trong đợt kèm mentor hiện tại (nếu có)
        var interns = await _context.BatchInterns
            .AsNoTracking()
            .Where(bi => bi.InternshipId == request.Id)
            .Include(bi => bi.Intern)
                .ThenInclude(u => u.InternProfile)
            .Select(bi => new BatchInternDto
            {
                InternId = bi.InternId,
                FullName = bi.Intern.FullName,
                Email = bi.Intern.Email,
                StudentCode = bi.Intern.InternProfile != null ? bi.Intern.InternProfile.StudentCode : null,
                University = bi.Intern.InternProfile != null ? bi.Intern.InternProfile.University : null,
                Major = bi.Intern.InternProfile != null ? bi.Intern.InternProfile.Major : null,
                Status = bi.Status,
                JoinedAt = bi.JoinedAt,
                Notes = bi.Notes,
                CurrentMentorId = _context.MentorAssignments
                    .Where(ma => ma.InternshipId == request.Id && ma.InternId == bi.InternId && ma.EndedAt == null)
                    .Select(ma => (Guid?)ma.MentorId)
                    .FirstOrDefault(),
                CurrentMentorName = _context.MentorAssignments
                    .Where(ma => ma.InternshipId == request.Id && ma.InternId == bi.InternId && ma.EndedAt == null)
                    .Select(ma => ma.BatchMentor.Mentor.FullName)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var detail = new InternshipDetailDto
        {
            Id = internship.Id,
            Name = internship.Name,
            StartDate = internship.StartDate,
            EndDate = internship.EndDate,
            Status = internship.Status,
            CreatedBy = internship.CreatedBy,
            CreatorName = internship.Creator.FullName,
            CreatedAt = internship.CreatedAt,
            UpdatedAt = internship.UpdatedAt,
            Mentors = mentors,
            Interns = interns
        };

        return ApiResponse<InternshipDetailDto>.Ok(detail);
    }
}
