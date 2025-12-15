using ACommerce.Admin.Authorization.Abstractions;
using ACommerce.Admin.Authorization.Entities;

namespace ACommerce.Admin.Authorization.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IBaseAsyncRepository<AdminUser> _userRepository;
    private readonly IAdminAuthService _authService;

    public AdminUserService(
        IBaseAsyncRepository<AdminUser> userRepository,
        IAdminAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<IEnumerable<AdminUser>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<AdminUser?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<AdminUser> CreateUserAsync(CreateAdminUserRequest request)
    {
        var existingUser = await _authService.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("البريد الإلكتروني مستخدم بالفعل");
        }

        var user = new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(user);
    }

    public async Task<AdminUser> UpdateUserAsync(Guid userId, UpdateAdminUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("المستخدم غير موجود");
        }

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.RoleId.HasValue) user.RoleId = request.RoleId.Value;

        user.UpdatedAt = DateTime.UtcNow;

        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (!await _authService.ValidatePasswordAsync(user, currentPassword))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}
