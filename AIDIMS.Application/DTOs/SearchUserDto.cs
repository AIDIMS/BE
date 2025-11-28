using AIDIMS.Domain.Enums;

namespace AIDIMS.Application.DTOs;

public class SearchUserDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole? Role { get; set; }
    public Department? Department { get; set; }
}
