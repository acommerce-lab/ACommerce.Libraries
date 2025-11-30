using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// المساحات المفضلة للمستخدم
/// </summary>
public class SpaceFavorite : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المستخدم
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// ملاحظات المستخدم
    /// </summary>
    public string? Notes { get; set; }
}
