using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<ApiResponse<UserInfoResponse>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserInfoResponse>>
{
    private readonly IInternFlowDbContext _context;

    public GetCurrentUserQueryHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<UserInfoResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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
