using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Engine;
using ACommerce.AccountingKernel.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.AccountingKernel.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// تسجيل المحرك المحاسبي.
    /// المطور يختار عبر الخيارات: هل يوثّق؟ هل يحفظ كيانات؟ هل ينشر أحداث؟
    /// ثم يسجل التطبيقات المناسبة (IEntryStore, IPersistenceGateway, IEventPublisher).
    /// </summary>
    public static IServiceCollection AddAccountingKernel(
        this IServiceCollection services,
        Action<EntryEngineOptions>? configure = null)
    {
        // الخيارات
        if (configure != null)
            services.Configure(configure);
        else
            services.Configure<EntryEngineOptions>(_ => { });

        // المحرك
        services.AddScoped<EntryEngine>();

        // التطبيقات الافتراضية (Null = لا شيء) - يستبدلها المطور إن أراد
        if (!services.Any(s => s.ServiceType == typeof(IEntryStore)))
            services.AddSingleton<IEntryStore, NullEntryStore>();

        if (!services.Any(s => s.ServiceType == typeof(IPersistenceGateway)))
            services.AddSingleton<IPersistenceGateway, NullPersistenceGateway>();

        if (!services.Any(s => s.ServiceType == typeof(IEventPublisher)))
            services.AddSingleton<IEventPublisher, NullEventPublisher>();

        return services;
    }
}

/// <summary>
/// ناشر أحداث فارغ - لا ينشر شيئاً
/// </summary>
internal class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync(object evt, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
