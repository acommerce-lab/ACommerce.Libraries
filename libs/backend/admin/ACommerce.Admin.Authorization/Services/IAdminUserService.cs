using ACommerce.Admin.Authorization.Entities;

namespace ACommerce.Admin.Authorization.Services;

public interface IAdminUserService
{
    Task<IEnumerable<AdminUser>> GetAllUsersAsync();
    Task<AdminUser?> GetUserByIdAsync(Guid userId);
    Task<AdminUser> CreateUserAsync(CreateAdminUserRequest request);
    Task<AdminUser> UpdateUserAsync(Guid userId, UpdateAdminUserRequest request);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(Guid userId, string newPassword);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
}

public class CreateAdminUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid RoleId { get; set; }
}

public class UpdateAdminUserRequest
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? RoleId { get; set; }
}
