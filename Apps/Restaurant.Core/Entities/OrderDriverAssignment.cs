using ACommerce.SharedKernel.Abstractions.Entities;

namespace Restaurant.Core.Entities;

/// <summary>
/// ربط السائق بالطلب - يتتبع دورة حياة التوصيل
/// </summary>
public class OrderDriverAssignment : IBaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف الطلب
    /// </summary>
    public Guid RestaurantOrderId { get; set; }

    /// <summary>
    /// الطلب
    /// </summary>
    public RestaurantOrder? RestaurantOrder { get; set; }

    /// <summary>
    /// معرف السائق
    /// </summary>
    public Guid DriverEmployeeId { get; set; }

    /// <summary>
    /// السائق
    /// </summary>
    public VendorEmployee? Driver { get; set; }

    // === التواريخ ===

    /// <summary>
    /// وقت تعيين السائق
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// وقت قبول السائق للطلب
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// وقت استلام الطلب (مسح الباركود)
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// وقت التسليم
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    // === الباركود ===

    /// <summary>
    /// الباركود الذي تم مسحه عند الاستلام
    /// </summary>
    public string? ScannedBarcode { get; set; }

    /// <summary>
    /// هل تم التحقق من صحة الباركود؟
    /// </summary>
    public bool BarcodeVerified { get; set; } = false;

    // === إثبات التسليم ===

    /// <summary>
    /// رابط صورة إثبات التسليم
    /// </summary>
    public string? DeliveryProofImageUrl { get; set; }

    /// <summary>
    /// ملاحظات التسليم من السائق
    /// </summary>
    public string? DeliveryNotes { get; set; }

    /// <summary>
    /// توقيع العميل (Base64 إذا وجد)
    /// </summary>
    public string? CustomerSignature { get; set; }

    // === الموقع ===

    /// <summary>
    /// خط العرض عند الاستلام
    /// </summary>
    public double? PickupLatitude { get; set; }

    /// <summary>
    /// خط الطول عند الاستلام
    /// </summary>
    public double? PickupLongitude { get; set; }

    /// <summary>
    /// خط العرض عند التسليم
    /// </summary>
    public double? DeliveryLatitude { get; set; }

    /// <summary>
    /// خط الطول عند التسليم
    /// </summary>
    public double? DeliveryLongitude { get; set; }

    // === الحالة ===

    /// <summary>
    /// هل تم إلغاء التعيين؟
    /// </summary>
    public bool IsCancelled { get; set; } = false;

    /// <summary>
    /// سبب الإلغاء
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// وقت الإلغاء
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    // === الدوال المساعدة ===

    /// <summary>
    /// هل تم استلام الطلب؟
    /// </summary>
    public bool IsPickedUp => PickedUpAt.HasValue;

    /// <summary>
    /// هل تم التسليم؟
    /// </summary>
    public bool IsDelivered => DeliveredAt.HasValue;

    /// <summary>
    /// حساب مدة التوصيل بالدقائق
    /// </summary>
    public int? GetDeliveryDurationMinutes()
    {
        if (!PickedUpAt.HasValue || !DeliveredAt.HasValue)
        {
            return null;
        }

        return (int)(DeliveredAt.Value - PickedUpAt.Value).TotalMinutes;
    }

    /// <summary>
    /// التحقق من صحة الباركود
    /// </summary>
    public bool VerifyBarcode(string expectedBarcode)
    {
        if (string.IsNullOrEmpty(ScannedBarcode) || string.IsNullOrEmpty(expectedBarcode))
        {
            return false;
        }

        BarcodeVerified = ScannedBarcode.Equals(expectedBarcode, StringComparison.OrdinalIgnoreCase);
        return BarcodeVerified;
    }

    /// <summary>
    /// تسجيل استلام الطلب
    /// </summary>
    public void RecordPickup(string barcode, double latitude, double longitude)
    {
        ScannedBarcode = barcode;
        PickedUpAt = DateTime.UtcNow;
        PickupLatitude = latitude;
        PickupLongitude = longitude;
    }

    /// <summary>
    /// تسجيل التسليم
    /// </summary>
    public void RecordDelivery(double latitude, double longitude, string? proofImageUrl = null, string? notes = null)
    {
        DeliveredAt = DateTime.UtcNow;
        DeliveryLatitude = latitude;
        DeliveryLongitude = longitude;
        DeliveryProofImageUrl = proofImageUrl;
        DeliveryNotes = notes;
    }
}
