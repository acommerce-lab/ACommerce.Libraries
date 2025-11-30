using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// تقييم المساحة
/// </summary>
public class SpaceReview : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// معرف الحجز المرتبط
    /// </summary>
    public Guid? BookingId { get; set; }
    public SpaceBooking? Booking { get; set; }

    /// <summary>
    /// معرف المقيّم
    /// </summary>
    public Guid ReviewerId { get; set; }

    /// <summary>
    /// اسم المقيّم
    /// </summary>
    public string? ReviewerName { get; set; }

    /// <summary>
    /// صورة المقيّم
    /// </summary>
    public string? ReviewerAvatar { get; set; }

    /// <summary>
    /// التقييم العام (1-5)
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// تقييم النظافة
    /// </summary>
    public int? CleanlinessRating { get; set; }

    /// <summary>
    /// تقييم الموقع
    /// </summary>
    public int? LocationRating { get; set; }

    /// <summary>
    /// تقييم المرافق
    /// </summary>
    public int? AmenitiesRating { get; set; }

    /// <summary>
    /// تقييم القيمة مقابل السعر
    /// </summary>
    public int? ValueRating { get; set; }

    /// <summary>
    /// تقييم التواصل
    /// </summary>
    public int? CommunicationRating { get; set; }

    /// <summary>
    /// عنوان التقييم
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// نص التقييم
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// الإيجابيات
    /// </summary>
    public string? Pros { get; set; }

    /// <summary>
    /// السلبيات
    /// </summary>
    public string? Cons { get; set; }

    /// <summary>
    /// رد المالك
    /// </summary>
    public string? OwnerResponse { get; set; }

    /// <summary>
    /// تاريخ رد المالك
    /// </summary>
    public DateTime? OwnerResponseAt { get; set; }

    /// <summary>
    /// تقييم معتمد
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// تقييم منشور
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// عدد الإعجابات
    /// </summary>
    public int HelpfulCount { get; set; }

    /// <summary>
    /// عدد البلاغات
    /// </summary>
    public int ReportCount { get; set; }
}
