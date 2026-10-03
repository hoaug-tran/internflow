using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Queries.GetInternships;

public class GetInternshipsQueryHandler : IRequestHandler<GetInternshipsQuery, ApiResponse<List<InternshipDto>>>
{
    private readonly IInternFlowDbContext _context;

    public GetInternshipsQueryHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<InternshipDto>>> Handle(GetInternshipsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Internships.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(search));
        }

        var page = request.Page > 0 ? request.Page : 1;
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InternshipDto
            {
                Id = i.Id,
                Name = i.Name,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Status = i.Status,
                CreatedBy = i.CreatedBy,
                CreatedAt = i.CreatedAt,
                MentorCount = i.BatchMentors.Count,
                InternCount = i.BatchInterns.Count
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<List<InternshipDto>>.Ok(items);
    }
}
