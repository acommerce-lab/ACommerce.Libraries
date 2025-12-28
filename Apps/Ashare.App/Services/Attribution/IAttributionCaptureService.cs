namespace Ashare.App.Services.Attribution;

/// <summary>
/// خدمة التقاط بيانات الإسناد التسويقي
/// تلتقط معلومات الحملات من الروابط العميقة وتحفظها للاستخدام لاحقاً
/// </summary>
public interface IAttributionCaptureService
{
    /// <summary>
    /// التقاط بيانات الإسناد من رابط
    /// </summary>
    /// <param name="url">الرابط المراد تحليله</param>
    Task CaptureFromUrlAsync(Uri url);

    /// <summary>
    /// التقاط بيانات الإسناد من رابط نصي
    /// </summary>
    Task CaptureFromUrlAsync(string url);

    /// <summary>
    /// الحصول على بيانات الإسناد المحفوظة
    /// </summary>
    /// <returns>بيانات الإسناد أو null إذا لم تكن موجودة أو منتهية</returns>
    Task<AttributionData?> GetAttributionAsync();

    /// <summary>
    /// مسح بيانات الإسناد المحفوظة
    /// </summary>
    Task ClearAttributionAsync();

    /// <summary>
    /// الحصول على Facebook Browser ID (_fbp)
    /// يُولّد إذا لم يكن موجوداً
    /// </summary>
    Task<string> GetOrCreateFbpAsync();

    /// <summary>
    /// الحصول على Facebook Click ID المُنسّق (_fbc)
    /// </summary>
    Task<string?> GetFbcAsync();

    /// <summary>
    /// حدث يُطلق عند التقاط بيانات إسناد جديدة
    /// </summary>
    event EventHandler<AttributionData>? AttributionCaptured;
}
