using AIDIMS.Domain.Common;

namespace AIDIMS.Domain.Entities;

/// <summary>
/// Sample User entity - Replace or extend as needed
/// </summary>
public class User : BaseAuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties can be added here
}
