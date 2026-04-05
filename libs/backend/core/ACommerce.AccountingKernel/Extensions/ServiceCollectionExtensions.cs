using ACommerce.AccountingKernel.Engine;
using ACommerce.AccountingKernel.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.AccountingKernel.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل المحرك المحاسبي مع الإعدادات الافتراضية.
    /// يستخدم NullEntryStore و NullPersistenceGateway (تنفيذ فقط بدون حفظ).
    /// </summary>
    public static IServiceCollection AddAccountingKernel(this IServiceCollection services)
    {
        services.TryAddScoped<EntryEngine>();
        services.TryAddScoped<IEntryStore, NullEntryStore>();
        services.TryAddScoped<IPersistenceGateway, NullPersistenceGateway>();
        return services;
    }

    /// <summary>
    /// تسجيل المحرك مع مخزن قيود مخصص (للتوثيق)
    /// </summary>
    public static IServiceCollection AddAccountingKernel<TEntryStore>(this IServiceCollection services)
        where TEntryStore : class, IEntryStore
    {
        services.TryAddScoped<EntryEngine>();
        services.AddScoped<IEntryStore, TEntryStore>();
        services.TryAddScoped<IPersistenceGateway, NullPersistenceGateway>();
        return services;
    }

    /// <summary>
    /// تسجيل المحرك مع مخزن قيود وبوابة حفظ مخصصة
    /// </summary>
    public static IServiceCollection AddAccountingKernel<TEntryStore, TPersistenceGateway>(this IServiceCollection services)
        where TEntryStore : class, IEntryStore
        where TPersistenceGateway : class, IPersistenceGateway
    {
        services.TryAddScoped<EntryEngine>();
        services.AddScoped<IEntryStore, TEntryStore>();
        services.AddScoped<IPersistenceGateway, TPersistenceGateway>();
        return services;
    }

    /// <summary>
    /// تسجيل المحرك مع مخزن في الذاكرة (للتطوير والاختبار)
    /// </summary>
    public static IServiceCollection AddAccountingKernelInMemory(this IServiceCollection services)
    {
        services.TryAddScoped<EntryEngine>();
        services.AddSingleton<IEntryStore, InMemoryEntryStore>();
        services.TryAddScoped<IPersistenceGateway, NullPersistenceGateway>();
        return services;
    }

    private static void TryAddScoped<TService, TImplementation>(this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        if (!services.Any(s => s.ServiceType == typeof(TService)))
            services.AddScoped<TService, TImplementation>();
    }

    private static void TryAddScoped<TService>(this IServiceCollection services)
        where TService : class
    {
        if (!services.Any(s => s.ServiceType == typeof(TService)))
            services.AddScoped<TService>();
    }
}
