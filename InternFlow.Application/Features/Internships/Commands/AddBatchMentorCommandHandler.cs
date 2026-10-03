using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchMentor;

public class AddBatchMentorCommandHandler : IRequestHandler<AddBatchMentorCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;

    public AddBatchMentorCommandHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(AddBatchMentorCommand request, CancellationToken cancellationToken)
    {
        var internship = await _context.Internships
            .FirstOrDefaultAsync(i => i.Id == request.InternshipId, cancellationToken);

        if (internship == null)
        {
            return ApiResponse<bool>.Fail("Đợt thực tập không tồn tại.");
        }

        if (internship.Status == InternshipStatus.Closed)
        {
            return ApiResponse<bool>.Fail("Không thể thêm Mentor vào đợt thực tập đã Đóng (Closed).");
        }

        var mentor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.MentorId, cancellationToken);

        if (mentor == null || !mentor.IsActive)
        {
            return ApiResponse<bool>.Fail("Người dùng không tồn tại hoặc đã bị vô hiệu hóa.");
        }

        if (mentor.Role != UserRole.Mentor)
        {
            return ApiResponse<bool>.Fail("Người dùng được chọn không có vai trò Mentor.");
        }

        var alreadyAdded = await _context.BatchMentors
            .AnyAsync(bm => bm.InternshipId == request.InternshipId && bm.MentorId == request.MentorId, cancellationToken);

        if (alreadyAdded)
        {
            return ApiResponse<bool>.Fail("Mentor này đã tham gia vào đợt thực tập từ trước.");
        }

        var batchMentor = new BatchMentor
        {
            InternshipId = request.InternshipId,
            MentorId = request.MentorId,
            MentorRole = UserRole.Mentor,
            AddedBy = request.AddedBy,
            JoinedAt = DateTime.UtcNow
        };

        _context.BatchMentors.Add(batchMentor);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Thêm Mentor vào đợt thực tập thành công.");
    }
}
