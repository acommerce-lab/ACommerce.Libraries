using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.ContactPoints.Entities;

/// <summary>
/// نقطة اتصال (بريد، هاتف، عنوان)
/// </summary>
public class ContactPoint : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المستخدم
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// نوع نقطة الاتصال
    /// </summary>
    public ContactPointType Type { get; set; }

    /// <summary>
    /// القيمة (رقم الهاتف، البريد، العنوان)
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// التصنيف (شخصي، عمل، إلخ)
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// هل هو الافتراضي؟
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// هل تم التحقق منه؟
    /// </summary>
    public bool IsVerified { get; set; }
}

public enum ContactPointType
{
    Email,
    Phone,
    Address,
    WhatsApp,
    Other
}
