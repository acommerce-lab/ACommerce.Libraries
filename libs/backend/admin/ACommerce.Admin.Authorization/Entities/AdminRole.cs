using ACommerce.Admin.Authorization.Abstractions;

namespace ACommerce.Admin.Authorization.Entities;

public class AdminRole : IBaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    
    public virtual ICollection<AdminUser> Users { get; set; } = new List<AdminUser>();
    public virtual ICollection<AdminRolePermission> RolePermissions { get; set; } = new List<AdminRolePermission>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
