using ACommerce.Admin.Authorization.Abstractions;

namespace ACommerce.Admin.Authorization.Entities;

public class AdminPermission : IBaseEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    
    public virtual ICollection<AdminRolePermission> RolePermissions { get; set; } = new List<AdminRolePermission>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
