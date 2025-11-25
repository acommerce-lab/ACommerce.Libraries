using ACommerce.ServiceRegistry.Abstractions.Models;

namespace ACommerce.ServiceRegistry.Abstractions.Interfaces;

/// <summary>
/// واجهة تخزين معلومات الخدمات
/// يمكن تنفيذها بـ InMemory أو Redis أو Database
/// </summary>
public interface IServiceStore
{
	/// <summary>
	/// حفظ أو تحديث خدمة
	/// </summary>
	Task SaveAsync(ServiceEndpoint endpoint, CancellationToken cancellationToken = default);

	/// <summary>
	/// حذف خدمة
	/// </summary>
	Task DeleteAsync(string serviceId, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على خدمة بالمعرف
	/// </summary>
	Task<ServiceEndpoint?> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default);

	/// <summary>
	/// الحصول على جميع الخدمات
	/// </summary>
	Task<IEnumerable<ServiceEndpoint>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// البحث عن خدمات بالاسم
	/// </summary>
	Task<IEnumerable<ServiceEndpoint>> FindByNameAsync(string serviceName, CancellationToken cancellationToken = default);

	/// <summary>
	/// تنظيف الخدمات القديمة/الميتة (Cleanup)
	/// </summary>
	Task CleanupStaleServicesAsync(TimeSpan threshold, CancellationToken cancellationToken = default);
}
