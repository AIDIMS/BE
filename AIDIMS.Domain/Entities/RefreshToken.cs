using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // Note: IsExpired and IsActive should be checked in service layer using IDateTimeProvider
    // to ensure correct timezone handling (Vietnam timezone vs UTC)
    // public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    // public bool IsActive => !IsRevoked && !IsExpired;
}
