using InternFlow.Domain.Entities;

namespace InternFlow.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    (string RawToken, string TokenHash, Guid FamilyId, DateTime ExpiresAt) GenerateRefreshToken(Guid? familyId = null);
    string HashToken(string token);
}
