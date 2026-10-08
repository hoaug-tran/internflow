using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;

public class AssignMentorCommandHandler : IRequestHandler<AssignMentorCommand, ApiResponse<long>>
{
    private readonly IInternFlowDbContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public AssignMentorCommandHandler(IInternFlowDbContext context) : this(context, null)
    {
    }

    public AssignMentorCommandHandler(IInternFlowDbContext context, ICurrentUserService? currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<long>> Handle(AssignMentorCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra đợt thực tập
        var internship = await _context.Internships
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.InternshipId, cancellationToken);

        if (internship == null)
        {
            return ApiResponse<long>.Fail("Đợt thực tập không tồn tại.");
        }

        if (internship.Status == InternshipStatus.Closed)
        {
            return ApiResponse<long>.Fail("Không thể phân công trong đợt thực tập đã Đóng.");
        }

        // 2. Đảm bảo cả Intern và Mentor đều đã thuộc đợt thực tập này
        var isInternInBatch = await _context.BatchInterns
            .AnyAsync(bi => bi.InternshipId == request.InternshipId && bi.InternId == request.InternId, cancellationToken);

        if (!isInternInBatch)
        {
            return ApiResponse<long>.Fail("Thực tập sinh chưa được thêm vào đợt thực tập này.");
        }

        var isMentorInBatch = await _context.BatchMentors
            .AnyAsync(bm => bm.InternshipId == request.InternshipId && bm.MentorId == request.MentorId, cancellationToken);

        if (!isMentorInBatch)
        {
            return ApiResponse<long>.Fail("Mentor chưa được thêm vào đợt thực tập này.");
        }

        // 3. Kiểm tra giới hạn MaxMentees của Mentor
        var mentorProfile = await _context.MentorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(mp => mp.UserId == request.MentorId, cancellationToken);

        if (mentorProfile?.MaxMentees != null)
        {
            var currentMenteesCount = await _context.MentorAssignments
                .CountAsync(ma => ma.MentorId == request.MentorId && ma.EndedAt == null, cancellationToken);

            if (currentMenteesCount >= mentorProfile.MaxMentees.Value)
            {
                return ApiResponse<long>.Fail($"Mentor đã đạt giới hạn tối đa ({mentorProfile.MaxMentees.Value}) sinh viên hướng dẫn.");
            }
        }

        // 4. Kiểm tra phân công hiện tại của Intern trong đợt
        var currentAssignment = await _context.MentorAssignments
            .FirstOrDefaultAsync(ma => ma.InternshipId == request.InternshipId && ma.InternId == request.InternId && ma.EndedAt == null, cancellationToken);

        if (currentAssignment != null)
        {
            if (currentAssignment.MentorId == request.MentorId)
            {
                return ApiResponse<long>.Ok(currentAssignment.Id, "Mentor này hiện đã và đang hướng dẫn thực tập sinh.");
            }

            // Tự động kết thúc phân công cũ để chuyển giao sang Mentor mới
            currentAssignment.EndedAt = DateTime.UtcNow;
            currentAssignment.EndReason = "Chuyển giao sang Mentor mới (Reassigned)";
        }

        var assignedBy = request.AssignedBy ?? _currentUserService?.UserId ?? Guid.Empty;

        // 5. Tạo phân công mới
        var newAssignment = new MentorAssignment
        {
            InternshipId = request.InternshipId,
            InternId = request.InternId,
            MentorId = request.MentorId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };

        _context.MentorAssignments.Add(newAssignment);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<long>.Ok(newAssignment.Id, "Phân công Mentor hướng dẫn thành công.");
    }
}
