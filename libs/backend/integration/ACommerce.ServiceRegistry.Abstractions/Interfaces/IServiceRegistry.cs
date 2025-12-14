using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Abstractions.Interfaces;

/// <summary>
/// واجهة تسجيل وإدارة الخدمات في Registry
/// </summary>
public interface IServiceRegistry
{
	/// <summary>
	/// تسجيل خدمة جديدة أو تحديث موجودة
	/// </summary>
	Task<ServiceEndpoint> RegisterAsync(ServiceRegistration registration, CancellationToken cancellationToken = default);

	/// <summary>
	/// إلغاء تسجيل خدمة
	/// </summary>
	Task<bool> DeregisterAsync(string serviceId, CancellationToken cancellationToken = default);

	/// <summary>
	/// تحديث حالة صحة خدمة
	/// </summary>
	Task UpdateHealthAsync(string serviceId, ServiceHealth health, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على خدمة محددة بالمعرف
	/// </summary>
	Task<ServiceEndpoint?> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على جميع الخدمات المسجلة
	/// </summary>
	Task<IEnumerable<ServiceEndpoint>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// تحديث heartbeat (نبضة القلب) للخدمة - تأكيد أنها ما زالت حية
	/// </summary>
	Task HeartbeatAsync(string serviceId, CancellationToken cancellationToken = default);
}
