using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Admin.AuditLog.Entities;

public class AuditLogEntry : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public AuditLogLevel Level { get; set; } = AuditLogLevel.Info;
    public string? Details { get; set; }
}

public enum AuditLogLevel
{
    Info,
    Warning,
    Error,
    Critical
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    Approve,
    Reject,
    Export,
    Import,
    Custom
}
