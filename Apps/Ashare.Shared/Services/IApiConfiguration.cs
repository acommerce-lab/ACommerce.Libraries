namespace Ashare.Shared.Services;

/// <summary>
/// واجهة إعدادات API الموحدة لجميع المنصات
/// </summary>
public interface IApiConfiguration
{
    /// <summary>
    /// عنوان API الأساسي
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// عنوان API كـ Uri
    /// </summary>
    Uri BaseUri { get; }

    /// <summary>
    /// هل نستخدم البيئة المحلية؟
    /// </summary>
    bool IsLocalEnvironment { get; }

    /// <summary>
    /// نوع المنصة الحالية
    /// </summary>
    AppPlatform Platform { get; }
}

/// <summary>
/// أنواع المنصات المدعومة
/// </summary>
public enum AppPlatform
{
    /// <summary>
    /// تطبيق ويب (Blazor Server/WebAssembly)
    /// </summary>
    Web,

    /// <summary>
    /// تطبيق أندرويد (MAUI)
    /// </summary>
    Android,

    /// <summary>
    /// تطبيق iOS (MAUI)
    /// </summary>
    iOS,

    /// <summary>
    /// تطبيق Windows (MAUI/WinUI)
    /// </summary>
    Windows,

    /// <summary>
    /// تطبيق macOS (MAUI)
    /// </summary>
    MacOS,

    /// <summary>
    /// غير محدد
    /// </summary>
    Unknown
}
