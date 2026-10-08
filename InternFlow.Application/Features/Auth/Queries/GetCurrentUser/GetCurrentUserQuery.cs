using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid? UserId = null) : IRequest<ApiResponse<UserInfoResponse>>;
