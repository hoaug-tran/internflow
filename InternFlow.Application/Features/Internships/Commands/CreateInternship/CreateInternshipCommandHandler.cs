using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Commands.CreateInternship;

public class CreateInternshipCommandHandler : IRequestHandler<CreateInternshipCommand, ApiResponse<InternshipDto>>
{
    private readonly IInternFlowDbContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public CreateInternshipCommandHandler(IInternFlowDbContext context) : this(context, null)
    {
    }

    public CreateInternshipCommandHandler(IInternFlowDbContext context, ICurrentUserService? currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<InternshipDto>> Handle(CreateInternshipCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<InternshipDto>.Fail("Tên đợt thực tập không được để trống.");
        }

        if (request.EndDate <= request.StartDate)
        {
            return ApiResponse<InternshipDto>.Fail("Ngày kết thúc đợt thực tập phải sau ngày bắt đầu.");
        }

        var normalizedName = request.Name.Trim();
        var exists = await _context.Internships
            .AnyAsync(i => i.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (exists)
        {
            return ApiResponse<InternshipDto>.Fail($"Đợt thực tập có tên '{normalizedName}' đã tồn tại trong hệ thống.");
        }

        var creatorId = request.CreatedBy ?? _currentUserService?.UserId ?? Guid.Empty;

        var internship = new Internship
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = InternshipStatus.Planned,
            CreatedBy = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Internships.Add(internship);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new InternshipDto
        {
            Id = internship.Id,
            Name = internship.Name,
            StartDate = internship.StartDate,
            EndDate = internship.EndDate,
            Status = internship.Status,
            CreatedBy = internship.CreatedBy,
            CreatedAt = internship.CreatedAt,
            MentorCount = 0,
            InternCount = 0
        };

        return ApiResponse<InternshipDto>.Ok(dto, "Tạo đợt thực tập mới thành công.");
    }
}
