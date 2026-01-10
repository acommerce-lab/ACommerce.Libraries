namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// خدمة ضغط الصور قبل الرفع
/// تقلل حجم الصور لتسريع الرفع وتوفير البيانات
/// </summary>
public interface IImageCompressionService
{
    /// <summary>
    /// ضغط صورة مع تحديد الحد الأقصى للأبعاد والجودة
    /// </summary>
    /// <param name="imageStream">الصورة الأصلية</param>
    /// <param name="maxWidth">أقصى عرض (افتراضي: 1920)</param>
    /// <param name="maxHeight">أقصى ارتفاع (افتراضي: 1920)</param>
    /// <param name="quality">جودة JPEG من 0-100 (افتراضي: 80)</param>
    /// <returns>الصورة المضغوطة كـ Stream</returns>
    Task<CompressedImageResult> CompressAsync(
        Stream imageStream,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80);

    /// <summary>
    /// ضغط صورة من مسار ملف
    /// </summary>
    Task<CompressedImageResult> CompressFromFileAsync(
        string filePath,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80);
}

/// <summary>
/// نتيجة ضغط الصورة
/// </summary>
public class CompressedImageResult
{
    /// <summary>
    /// الصورة المضغوطة
    /// </summary>
    public Stream? Stream { get; set; }

    /// <summary>
    /// الحجم الأصلي بالبايت
    /// </summary>
    public long OriginalSize { get; set; }

    /// <summary>
    /// الحجم بعد الضغط بالبايت
    /// </summary>
    public long CompressedSize { get; set; }

    /// <summary>
    /// العرض الأصلي
    /// </summary>
    public int OriginalWidth { get; set; }

    /// <summary>
    /// الارتفاع الأصلي
    /// </summary>
    public int OriginalHeight { get; set; }

    /// <summary>
    /// العرض بعد الضغط
    /// </summary>
    public int NewWidth { get; set; }

    /// <summary>
    /// الارتفاع بعد الضغط
    /// </summary>
    public int NewHeight { get; set; }

    /// <summary>
    /// نسبة التوفير في الحجم
    /// </summary>
    public double CompressionRatio => OriginalSize > 0
        ? (1 - (double)CompressedSize / OriginalSize) * 100
        : 0;

    /// <summary>
    /// هل نجح الضغط؟
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// رسالة الخطأ إذا فشل الضغط
    /// </summary>
    public string? ErrorMessage { get; set; }
}
