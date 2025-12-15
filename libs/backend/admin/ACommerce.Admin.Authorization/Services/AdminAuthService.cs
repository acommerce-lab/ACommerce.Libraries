using ACommerce.Admin.Authorization.Abstractions;
using ACommerce.Admin.Authorization.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ACommerce.Admin.Authorization.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IBaseAsyncRepository<AdminUser> _userRepository;
    private readonly IBaseAsyncRepository<AdminRole> _roleRepository;
    private readonly IBaseAsyncRepository<AdminRolePermission> _rolePermissionRepository;
    private readonly IConfiguration _configuration;

    public AdminAuthService(
        IBaseAsyncRepository<AdminUser> userRepository,
        IBaseAsyncRepository<AdminRole> roleRepository,
        IBaseAsyncRepository<AdminRolePermission> rolePermissionRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _configuration = configuration;
    }

    public async Task<AdminLoginResult> LoginAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
        {
            return new AdminLoginResult { Success = false, ErrorMessage = "البريد الإلكتروني أو كلمة المرور غير صحيحة" };
        }

        if (!user.IsActive)
        {
            return new AdminLoginResult { Success = false, ErrorMessage = "الحساب غير مفعل" };
        }

        if (!await ValidatePasswordAsync(user, password))
        {
            return new AdminLoginResult { Success = false, ErrorMessage = "البريد الإلكتروني أو كلمة المرور غير صحيحة" };
        }

        await UpdateLastLoginAsync(user.Id);

        var token = await GenerateTokenAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        return new AdminLoginResult
        {
            Success = true,
            Token = token,
            User = new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                RoleName = user.Role?.Name ?? "",
                Permissions = permissions
            }
        };
    }

    public async Task<AdminUser?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<AdminUser?> GetUserByEmailAsync(string email)
    {
        var users = await _userRepository.FindAsync(u => u.Email == email);
        return users.FirstOrDefault();
    }

    public Task<bool> ValidatePasswordAsync(AdminUser user, string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
    }

    public async Task<string> GenerateTokenAsync(AdminUser user)
    {
        var permissions = await GetUserPermissionsAsync(user.Id);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("role_id", user.RoleId.ToString()),
            new("role_name", user.Role?.Name ?? ""),
            new("is_admin", "true")
        };

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Secret"] ?? "AdminSecretKeyForJwtTokenGeneration123!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "ACommerce.Admin",
            audience: _configuration["Jwt:Audience"] ?? "ACommerce.Admin",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionCode);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return Enumerable.Empty<string>();

        var rolePermissions = await _rolePermissionRepository.FindAsync(
            rp => rp.RoleId == user.RoleId);

        return rolePermissions
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Code)
            .Distinct();
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }
    }
}
