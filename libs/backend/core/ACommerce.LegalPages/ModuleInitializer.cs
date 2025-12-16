using System.Runtime.CompilerServices;
using ACommerce.LegalPages.Entities;

namespace ACommerce.LegalPages;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        _ = typeof(LegalPage).Assembly;
    }
}
