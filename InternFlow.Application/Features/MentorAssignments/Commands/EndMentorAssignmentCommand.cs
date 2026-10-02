using InternFlow.Application.Common.Interfaces;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.MentorAssignments.Commands.EndMentorAssignment;

public record EndMentorAssignmentCommand(
    long AssignmentId,
    string? EndReason = null
) : IRequest<ApiResponse<bool>>;

public class EndMentorAssignmentCommandHandler : IRequestHandler<EndMentorAssignmentCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;

    public EndMentorAssignmentCommandHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(EndMentorAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.MentorAssignments
            .FirstOrDefaultAsync(ma => ma.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy thông tin phân công.");
        }

        if (assignment.EndedAt != null)
        {
            return ApiResponse<bool>.Fail("Phân công hướng dẫn này đã kết thúc từ trước.");
        }

        assignment.EndedAt = DateTime.UtcNow;
        assignment.EndReason = !string.IsNullOrWhiteSpace(request.EndReason)
            ? request.EndReason.Trim()
            : "Kết thúc đợt hướng dẫn";

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, "Kết thúc phân công hướng dẫn thành công.");
    }
}
