namespace Ashare.Shared.Services;

/// <summary>
/// خدمة اختيار الوسائط (صور/فيديو) من الكاميرا أو المعرض
/// </summary>
public interface IMediaPickerService
{
    /// <summary>
    /// التحقق من توفر الكاميرا
    /// </summary>
    bool IsCameraAvailable { get; }

    /// <summary>
    /// التقاط صورة من الكاميرا
    /// </summary>
    Task<MediaPickResult?> CapturePhotoAsync();

    /// <summary>
    /// اختيار صورة من المعرض
    /// </summary>
    Task<MediaPickResult?> PickPhotoAsync();

    /// <summary>
    /// اختيار صور متعددة من المعرض
    /// </summary>
    Task<List<MediaPickResult>> PickPhotosAsync(int maxCount = 10);

    /// <summary>
    /// عرض خيارات الكاميرا والمعرض للمستخدم
    /// </summary>
    Task<List<MediaPickResult>> PickOrCapturePhotosAsync(int maxCount = 10);
}

/// <summary>
/// نتيجة اختيار الوسائط
/// </summary>
public class MediaPickResult
{
    /// <summary>
    /// مسار الملف المحلي
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// اسم الملف
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// نوع المحتوى (MIME type)
    /// </summary>
    public string ContentType { get; set; } = "image/jpeg";

    /// <summary>
    /// Stream للقراءة - يجب استخدامه فوراً ثم التخلص منه
    /// </summary>
    public Func<Task<Stream>>? OpenReadAsync { get; set; }
}
