using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserInfoResponse>>
{
    private readonly IInternFlowDbContext _context;
    private readonly ICurrentUserService? _currentUserService;

    public GetCurrentUserQueryHandler(IInternFlowDbContext context) : this(context, null)
    {
    }

    public GetCurrentUserQueryHandler(IInternFlowDbContext context, ICurrentUserService? currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<UserInfoResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? _currentUserService?.UserId;
        if (!targetUserId.HasValue)
        {
            return ApiResponse<UserInfoResponse>.Fail("Không xác định được danh tính người dùng");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId.Value, cancellationToken);

        if (user == null)
        {
            return ApiResponse<UserInfoResponse>.Fail("Không tìm thấy người dùng");
        }

        var response = new UserInfoResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        };

        return ApiResponse<UserInfoResponse>.Ok(response);
    }
}
