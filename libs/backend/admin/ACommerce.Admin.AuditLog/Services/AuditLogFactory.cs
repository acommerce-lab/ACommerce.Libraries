using System.Security.Claims;
using ACommerce.Admin.AuditLog.DTOs;
using ACommerce.Admin.AuditLog.Entities;

namespace ACommerce.Admin.AuditLog.Services;

/// <summary>
/// مصنع موحّد لبناء <see cref="CreateAuditLogDto"/> من هوية المشرف الحالي.
/// يُستخدم من متحكّمات الإدارة لكتابة سجل تدقيق متّسق بعد كل عملية.
/// </summary>
public static class AuditLogFactory
{
    public static CreateAuditLogDto ForAdmin(
        ClaimsPrincipal? user,
        string action,
        string entityType,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? details = null,
        AuditLogLevel level = AuditLogLevel.Info,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new CreateAuditLogDto
        {
            UserId = TryGetUserId(user),
            UserName = GetClaim(user, ClaimTypes.Name, "name", "preferred_username"),
            UserEmail = GetClaim(user, ClaimTypes.Email, "email"),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            Details = details,
            Level = level,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }

    private static Guid? TryGetUserId(ClaimsPrincipal? user)
    {
        var raw = GetClaim(user, ClaimTypes.NameIdentifier, "sub", "uid", "userId");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static string? GetClaim(ClaimsPrincipal? user, params string[] types)
    {
        if (user is null) return null;
        foreach (var type in types)
        {
            var value = user.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }
        return null;
    }
}
