using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// المساحة المشتركة
/// </summary>
public class Space : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// اسم المساحة
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// الرابط المختصر (Slug)
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// نوع المساحة
    /// </summary>
    public SpaceType Type { get; set; } = SpaceType.CoWorking;

    /// <summary>
    /// حالة المساحة
    /// </summary>
    public SpaceStatus Status { get; set; } = SpaceStatus.Draft;

    /// <summary>
    /// وصف مختصر
    /// </summary>
    public string? ShortDescription { get; set; }

    /// <summary>
    /// وصف تفصيلي
    /// </summary>
    public string? LongDescription { get; set; }

    /// <summary>
    /// السعة القصوى (عدد الأشخاص)
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// الحد الأدنى لعدد الأشخاص
    /// </summary>
    public int MinCapacity { get; set; } = 1;

    /// <summary>
    /// المساحة بالمتر المربع
    /// </summary>
    public decimal? AreaSquareMeters { get; set; }

    /// <summary>
    /// رقم الطابق
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// رقم المبنى/الغرفة
    /// </summary>
    public string? RoomNumber { get; set; }

    /// <summary>
    /// معرف المالك/المزود
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// معرف الموقع الجغرافي
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// العنوان الكامل
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// خط العرض
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// المدينة
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// الحي
    /// </summary>
    public string? District { get; set; }

    /// <summary>
    /// الرمز البريدي
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// صورة رئيسية
    /// </summary>
    public string? FeaturedImage { get; set; }

    /// <summary>
    /// معرض الصور
    /// </summary>
    [NotMapped]
    public List<string> Images { get; set; } = new();

    /// <summary>
    /// فيديوهات
    /// </summary>
    [NotMapped]
    public List<string> Videos { get; set; } = new();

    /// <summary>
    /// جولة افتراضية (رابط)
    /// </summary>
    public string? VirtualTourUrl { get; set; }

    /// <summary>
    /// يتطلب موافقة فورية أم لا
    /// </summary>
    public bool InstantBooking { get; set; } = true;

    /// <summary>
    /// الحد الأدنى لمدة الحجز (بالدقائق)
    /// </summary>
    public int MinBookingDurationMinutes { get; set; } = 60;

    /// <summary>
    /// الحد الأقصى لمدة الحجز (بالدقائق)
    /// </summary>
    public int? MaxBookingDurationMinutes { get; set; }

    /// <summary>
    /// وقت الإشعار المسبق المطلوب (بالساعات)
    /// </summary>
    public int AdvanceNoticeHours { get; set; } = 24;

    /// <summary>
    /// سياسة الإلغاء
    /// </summary>
    public string? CancellationPolicy { get; set; }

    /// <summary>
    /// ساعات الإلغاء المجاني قبل الموعد
    /// </summary>
    public int? FreeCancellationHours { get; set; } = 24;

    /// <summary>
    /// قواعد المساحة
    /// </summary>
    public string? HouseRules { get; set; }

    /// <summary>
    /// تعليمات الوصول
    /// </summary>
    public string? AccessInstructions { get; set; }

    /// <summary>
    /// رقم الاتصال
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// البريد الإلكتروني
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// متوسط التقييم
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>
    /// عدد التقييمات
    /// </summary>
    public int ReviewsCount { get; set; }

    /// <summary>
    /// عدد الحجوزات المكتملة
    /// </summary>
    public int CompletedBookingsCount { get; set; }

    /// <summary>
    /// مميز
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// موثق
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// كلمات مفتاحية (SEO)
    /// </summary>
    [NotMapped]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// بيانات إضافية
    /// </summary>
    [NotMapped]
    public Dictionary<string, string> Metadata { get; set; } = new();

    // ====================================================================================
    // العلاقات
    // ====================================================================================

    /// <summary>
    /// المرافق والخدمات
    /// </summary>
    public List<SpaceAmenity> Amenities { get; set; } = new();

    /// <summary>
    /// الأسعار
    /// </summary>
    public List<SpacePrice> Prices { get; set; } = new();

    /// <summary>
    /// أوقات العمل
    /// </summary>
    public List<SpaceAvailability> Availabilities { get; set; } = new();

    /// <summary>
    /// الحجوزات
    /// </summary>
    public List<SpaceBooking> Bookings { get; set; } = new();

    /// <summary>
    /// التقييمات
    /// </summary>
    public List<SpaceReview> Reviews { get; set; } = new();
}
