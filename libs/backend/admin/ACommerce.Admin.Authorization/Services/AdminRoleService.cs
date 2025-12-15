using ACommerce.Admin.Authorization.Abstractions;
using ACommerce.Admin.Authorization.Entities;

namespace ACommerce.Admin.Authorization.Services;

public class AdminRoleService : IAdminRoleService
{
    private readonly IBaseAsyncRepository<AdminRole> _roleRepository;
    private readonly IBaseAsyncRepository<AdminPermission> _permissionRepository;
    private readonly IBaseAsyncRepository<AdminRolePermission> _rolePermissionRepository;

    public AdminRoleService(
        IBaseAsyncRepository<AdminRole> roleRepository,
        IBaseAsyncRepository<AdminPermission> permissionRepository,
        IBaseAsyncRepository<AdminRolePermission> rolePermissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
    }

    public async Task<IEnumerable<AdminRole>> GetAllRolesAsync()
    {
        return await _roleRepository.GetAllAsync();
    }

    public async Task<AdminRole?> GetRoleByIdAsync(Guid roleId)
    {
        return await _roleRepository.GetByIdAsync(roleId);
    }

    public async Task<AdminRole> CreateRoleAsync(CreateAdminRoleRequest request)
    {
        var role = new AdminRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRole = await _roleRepository.AddAsync(role);

        if (request.PermissionIds.Any())
        {
            await SetRolePermissionsAsync(createdRole.Id, request.PermissionIds);
        }

        return createdRole;
    }

    public async Task<AdminRole> UpdateRoleAsync(Guid roleId, UpdateAdminRoleRequest request)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
        {
            throw new InvalidOperationException("الدور غير موجود");
        }

        if (role.IsSystem)
        {
            throw new InvalidOperationException("لا يمكن تعديل دور النظام");
        }

        if (request.Name != null) role.Name = request.Name;
        if (request.Description != null) role.Description = request.Description;

        role.UpdatedAt = DateTime.UtcNow;

        var updatedRole = await _roleRepository.UpdateAsync(role);

        if (request.PermissionIds != null)
        {
            await SetRolePermissionsAsync(roleId, request.PermissionIds);
        }

        return updatedRole;
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null) return false;

        if (role.IsSystem)
        {
            throw new InvalidOperationException("لا يمكن حذف دور النظام");
        }

        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role);
        return true;
    }

    public async Task<IEnumerable<AdminPermission>> GetRolePermissionsAsync(Guid roleId)
    {
        var rolePermissions = await _rolePermissionRepository.FindAsync(rp => rp.RoleId == roleId);
        return rolePermissions
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!);
    }

    public async Task<bool> SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        var existingPermissions = await _rolePermissionRepository.FindAsync(rp => rp.RoleId == roleId);
        foreach (var existing in existingPermissions)
        {
            existing.IsDeleted = true;
            await _rolePermissionRepository.UpdateAsync(existing);
        }

        foreach (var permissionId in permissionIds)
        {
            var rolePermission = new AdminRolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _rolePermissionRepository.AddAsync(rolePermission);
        }

        return true;
    }
}
