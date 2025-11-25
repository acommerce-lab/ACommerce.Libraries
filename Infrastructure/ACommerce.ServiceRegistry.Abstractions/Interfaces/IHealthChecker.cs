using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Abstractions.Interfaces;

/// <summary>
/// واجهة فحص صحة الخدمات (Health Checker)
/// </summary>
public interface IHealthChecker
{
	/// <summary>
	/// فحص صحة خدمة واحدة
	/// </summary>
	Task<ServiceHealth> CheckHealthAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default);

	/// <summary>
	/// فحص صحة عدة خدمات
	/// </summary>
	Task<Dictionary<string, ServiceHealth>> CheckMultipleAsync(IEnumerable<ServiceEndpoint> endpoints, CancellationToken cancellationToken = default);
}
