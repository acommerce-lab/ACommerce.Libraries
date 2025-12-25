// Re-export from ACommerce.Client.Core for backward compatibility
// New code should use ACommerce.Client.Core.Configuration namespace directly
global using IApiConfiguration = ACommerce.Client.Core.Configuration.IApiConfiguration;
global using AppPlatform = ACommerce.Client.Core.Configuration.AppPlatform;

namespace Ashare.Shared.Services;

/// <summary>
/// هذا الملف يعيد تصدير IApiConfiguration و AppPlatform من Client.Core للتوافق العكسي
/// الكود الجديد يجب أن يستخدم ACommerce.Client.Core.Configuration مباشرة
/// </summary>
public static class ApiConfigurationInterfaceAliases
{
    // Aliases are defined via global using above
}
