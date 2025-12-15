using ACommerce.Admin.Authorization.Abstractions;

namespace ACommerce.Admin.Authorization.Entities;

public class AdminRolePermission : IBaseEntity
{
    public Guid Id { get; set; }
    
    public Guid RoleId { get; set; }
    public virtual AdminRole? Role { get; set; }
    
    public Guid PermissionId { get; set; }
    public virtual AdminPermission? Permission { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
