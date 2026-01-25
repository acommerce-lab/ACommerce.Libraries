namespace ACommerce.Subscriptions.Enums;

/// <summary>
/// حالة الاشتراك
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>تجريبي - في فترة التجربة المجانية</summary>
    Trial = 1,

    /// <summary>نشط - الاشتراك فعال ومدفوع</summary>
    Active = 2,

    /// <summary>متأخر السداد - اشتراك كان نشطاً وتأخر في التجديد</summary>
    PastDue = 3,

    /// <summary>ملغي - تم إلغاء الاشتراك</summary>
    Cancelled = 4,

    /// <summary>منتهي - انتهت صلاحية الاشتراك</summary>
    Expired = 5,

    /// <summary>موقوف - تم إيقافه من الإدارة</summary>
    Suspended = 6,

    /// <summary>في انتظار السداد - اشتراك جديد في انتظار الدفع الأول</summary>
    PendingPayment = 7
}

/// <summary>
/// دورة الفوترة
/// </summary>
public enum BillingCycle
{
    /// <summary>شهري</summary>
    Monthly = 1,

    /// <summary>ربع سنوي (كل 3 أشهر)</summary>
    Quarterly = 3,

    /// <summary>نصف سنوي (كل 6 أشهر)</summary>
    SemiAnnual = 6,

    /// <summary>سنوي</summary>
    Annual = 12
}

/// <summary>
/// نوع العمولة
/// </summary>
public enum CommissionType
{
    /// <summary>نسبة مئوية من قيمة العملية</summary>
    Percentage = 1,

    /// <summary>مبلغ ثابت لكل عملية</summary>
    Fixed = 2,

    /// <summary>نسبة مئوية + مبلغ ثابت</summary>
    Hybrid = 3
}

/// <summary>
/// مستوى الدعم الفني
/// </summary>
public enum SupportLevel
{
    /// <summary>دعم أساسي - بريد إلكتروني فقط</summary>
    Basic = 1,

    /// <summary>دعم قياسي - بريد + دردشة</summary>
    Standard = 2,

    /// <summary>دعم أولوية - استجابة سريعة</summary>
    Priority = 3,

    /// <summary>دعم VIP - مدير حساب مخصص</summary>
    Dedicated = 4
}

/// <summary>
/// مستوى الإحصائيات
/// </summary>
public enum AnalyticsLevel
{
    /// <summary>بدون إحصائيات</summary>
    None = 0,

    /// <summary>إحصائيات أساسية (مشاهدات، نقرات)</summary>
    Basic = 1,

    /// <summary>إحصائيات متقدمة (تحويلات، مصادر)</summary>
    Advanced = 2,

    /// <summary>إحصائيات كاملة مع تقارير مخصصة</summary>
    Full = 3
}

/// <summary>
/// نوع الحد
/// </summary>
public enum LimitType
{
    /// <summary>عدد العروض</summary>
    Listings = 1,

    /// <summary>صور لكل عرض</summary>
    ImagesPerListing = 2,

    /// <summary>عروض مميزة</summary>
    FeaturedListings = 3,

    /// <summary>مساحة التخزين بالميجابايت</summary>
    StorageMB = 4,

    /// <summary>عدد الموظفين</summary>
    TeamMembers = 5,

    /// <summary>رسائل شهرية</summary>
    MonthlyMessages = 6,

    /// <summary>طلبات API شهرية</summary>
    MonthlyApiCalls = 7
}

/// <summary>
/// حالة الفاتورة
/// </summary>
public enum InvoiceStatus
{
    /// <summary>مسودة</summary>
    Draft = 1,

    /// <summary>في انتظار الدفع</summary>
    Pending = 2,

    /// <summary>مدفوعة</summary>
    Paid = 3,

    /// <summary>فشل الدفع</summary>
    Failed = 4,

    /// <summary>مستردة</summary>
    Refunded = 5,

    /// <summary>ملغاة</summary>
    Cancelled = 6
}

/// <summary>
/// نوع حدث الاشتراك
/// </summary>
public enum SubscriptionEventType
{
    /// <summary>إنشاء اشتراك جديد</summary>
    Created = 1,

    /// <summary>تفعيل الاشتراك</summary>
    Activated = 2,

    /// <summary>تجديد الاشتراك</summary>
    Renewed = 3,

    /// <summary>ترقية الباقة</summary>
    Upgraded = 4,

    /// <summary>تخفيض الباقة</summary>
    Downgraded = 5,

    /// <summary>إلغاء الاشتراك</summary>
    Cancelled = 6,

    /// <summary>انتهاء الاشتراك</summary>
    Expired = 7,

    /// <summary>إيقاف من الإدارة</summary>
    Suspended = 8,

    /// <summary>إعادة تفعيل</summary>
    Reactivated = 9,

    /// <summary>فشل الدفع</summary>
    PaymentFailed = 10,

    /// <summary>نجاح الدفع</summary>
    PaymentSucceeded = 11
}
