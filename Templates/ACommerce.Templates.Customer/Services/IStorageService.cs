// Re-export from Client.Core for backward compatibility
// New code should use ACommerce.Client.Core.Storage namespace directly
global using IStorageService = ACommerce.Client.Core.Storage.IStorageService;
global using InMemoryStorageService = ACommerce.Client.Core.Storage.InMemoryStorageService;

namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// هذا الملف يعيد تصدير IStorageService من Client.Core للتوافق العكسي
/// الكود الجديد يجب أن يستخدم ACommerce.Client.Core.Storage مباشرة
/// </summary>
public static class StorageServiceAliases
{
    // Aliases are defined via global using above
}
