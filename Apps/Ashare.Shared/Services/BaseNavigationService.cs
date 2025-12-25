// Re-export from ACommerce.Templates.Customer for backward compatibility
// New code should use ACommerce.Templates.Customer.Services.BaseNavigationService directly
global using BaseNavigationService = ACommerce.Templates.Customer.Services.BaseNavigationService;

namespace Ashare.Shared.Services;

/// <summary>
/// هذا الملف يعيد تصدير BaseNavigationService من Templates.Customer للتوافق العكسي
/// الكود الجديد يجب أن يستخدم ACommerce.Templates.Customer.Services.BaseNavigationService مباشرة
/// </summary>
public static class BaseNavigationServiceAliases
{
    // Aliases are defined via global using above
}
