// Re-export from ACommerce.Client.Auth for backward compatibility
// New code should use ACommerce.Client.Auth.StorageBackedTokenStorage directly
global using TokenStorageService = ACommerce.Client.Auth.StorageBackedTokenStorage;

namespace Ashare.Shared.Services;

/// <summary>
/// هذا الملف يعيد تصدير StorageBackedTokenStorage من Client.Auth للتوافق العكسي
/// الكود الجديد يجب أن يستخدم ACommerce.Client.Auth.StorageBackedTokenStorage مباشرة
/// </summary>
public static class TokenStorageServiceAliases
{
    // Aliases are defined via global using above
}
