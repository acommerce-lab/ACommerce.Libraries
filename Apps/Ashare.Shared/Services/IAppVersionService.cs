namespace Ashare.Shared.Services;

/// <summary>
/// واجهة للحصول على معلومات إصدار التطبيق
/// يتم تنفيذها بشكل مختلف في كل منصة (MAUI, Web, etc.)
/// </summary>
public interface IAppVersionService
{
    /// <summary>
    /// رقم الإصدار (مثل "1.0.0")
    /// </summary>
    string Version { get; }

    /// <summary>
    /// رقم البناء (مثل "1" أو "100")
    /// </summary>
    string Build { get; }

    /// <summary>
    /// رقم البناء كعدد صحيح
    /// </summary>
    int BuildNumber { get; }

    /// <summary>
    /// اسم الحزمة (مثل "com.ashare.ashare")
    /// </summary>
    string PackageName { get; }

    /// <summary>
    /// هل هذا تطبيق موبايل (MAUI)؟
    /// </summary>
    bool IsMobileApp { get; }
}
