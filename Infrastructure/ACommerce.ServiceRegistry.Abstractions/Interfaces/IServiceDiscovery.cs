using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Abstractions.Interfaces;

/// <summary>
/// واجهة اكتشاف الخدمات (Service Discovery)
/// تستخدم من قبل التطبيقات للبحث عن الخدمات
/// </summary>
public interface IServiceDiscovery
{
	/// <summary>
	/// البحث عن خدمة بناءً على الاستعلام
	/// </summary>
	Task<ServiceEndpoint?> DiscoverAsync(ServiceQuery query, CancellationToken cancellationToken = default);

	/// <summary>
	/// البحث عن كل الخدمات التي تطابق الاستعلام
	/// </summary>
	Task<IEnumerable<ServiceEndpoint>> DiscoverAllAsync(ServiceQuery query, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على خدمة باسمها (مع Load Balancing إن وجد أكثر من نسخة)
	/// </summary>
	Task<ServiceEndpoint?> GetServiceAsync(string serviceName, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على جميع نسخ خدمة معينة
	/// </summary>
	Task<IEnumerable<ServiceEndpoint>> GetAllInstancesAsync(string serviceName, CancellationToken cancellationToken = default);
}
