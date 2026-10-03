using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchIntern;

public class AddBatchInternCommandHandler : IRequestHandler<AddBatchInternCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;

    public AddBatchInternCommandHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(AddBatchInternCommand request, CancellationToken cancellationToken)
    {
        var internship = await _context.Internships
            .FirstOrDefaultAsync(i => i.Id == request.InternshipId, cancellationToken);

        if (internship == null)
        {
            return ApiResponse<bool>.Fail("Đợt thực tập không tồn tại.");
        }

        if (internship.Status == InternshipStatus.Closed)
        {
            return ApiResponse<bool>.Fail("Không thể thêm thực tập sinh vào đợt thực tập đã Đóng (Closed).");
        }

        var intern = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.InternId, cancellationToken);

        if (intern == null || !intern.IsActive)
        {
            return ApiResponse<bool>.Fail("Người dùng không tồn tại hoặc đã bị vô hiệu hóa.");
        }

        if (intern.Role != UserRole.Intern)
        {
            return ApiResponse<bool>.Fail("Người dùng được chọn không có vai trò Intern.");
        }

        var alreadyAdded = await _context.BatchInterns
            .AnyAsync(bi => bi.InternshipId == request.InternshipId && bi.InternId == request.InternId, cancellationToken);

        if (alreadyAdded)
        {
            return ApiResponse<bool>.Fail("Thực tập sinh này đã tham gia vào đợt thực tập từ trước.");
        }

        var batchIntern = new BatchIntern
        {
            InternshipId = request.InternshipId,
            InternId = request.InternId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = request.AddedBy,
            JoinedAt = DateTime.UtcNow,
            Notes = request.Notes
        };

        _context.BatchInterns.Add(batchIntern);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, "Thêm thực tập sinh vào đợt thực tập thành công.");
    }
}
