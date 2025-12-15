using ACommerce.Admin.Authorization.Entities;

namespace ACommerce.Admin.Authorization.Services;

public interface IAdminRoleService
{
    Task<IEnumerable<AdminRole>> GetAllRolesAsync();
    Task<AdminRole?> GetRoleByIdAsync(Guid roleId);
    Task<AdminRole> CreateRoleAsync(CreateAdminRoleRequest request);
    Task<AdminRole> UpdateRoleAsync(Guid roleId, UpdateAdminRoleRequest request);
    Task<bool> DeleteRoleAsync(Guid roleId);
    Task<IEnumerable<AdminPermission>> GetRolePermissionsAsync(Guid roleId);
    Task<bool> SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
}

public class CreateAdminRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IEnumerable<Guid> PermissionIds { get; set; } = new List<Guid>();
}

public class UpdateAdminRoleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public IEnumerable<Guid>? PermissionIds { get; set; }
}
