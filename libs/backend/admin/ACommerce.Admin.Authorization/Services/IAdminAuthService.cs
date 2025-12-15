using ACommerce.Admin.Authorization.Entities;

namespace ACommerce.Admin.Authorization.Services;

public interface IAdminAuthService
{
    Task<AdminLoginResult> LoginAsync(string email, string password);
    Task<AdminUser?> GetUserByIdAsync(Guid userId);
    Task<AdminUser?> GetUserByEmailAsync(string email);
    Task<bool> ValidatePasswordAsync(AdminUser user, string password);
    Task<string> GenerateTokenAsync(AdminUser user);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    Task UpdateLastLoginAsync(Guid userId);
}

public class AdminLoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public AdminUserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}
