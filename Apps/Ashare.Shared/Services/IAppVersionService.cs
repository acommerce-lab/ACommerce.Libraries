// Re-export from ACommerce.Templates.Customer for backward compatibility
// New code should use ACommerce.Templates.Customer.Services.IAppVersionService directly
global using IAppVersionService = ACommerce.Templates.Customer.Services.IAppVersionService;

namespace Ashare.Shared.Services;

/// <summary>
/// هذا الملف يعيد تصدير IAppVersionService من Templates.Customer للتوافق العكسي
/// الكود الجديد يجب أن يستخدم ACommerce.Templates.Customer.Services.IAppVersionService مباشرة
/// </summary>
public static class AppVersionServiceAliases
{
    // Aliases are defined via global using above
}
