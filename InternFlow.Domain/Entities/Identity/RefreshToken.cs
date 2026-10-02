namespace InternFlow.Domain.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime? UsedAt { get; set; }
    public long? ReplacedByTokenId { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }

    // === Helper Properties (Không lưu trong DB) ===
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // === Navigation Properties ===
    public User User { get; set; } = null!;
    public RefreshToken? ReplacedByToken { get; set; }
}
