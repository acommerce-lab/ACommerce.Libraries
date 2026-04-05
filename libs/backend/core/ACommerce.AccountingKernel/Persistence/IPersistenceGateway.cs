using ACommerce.AccountingKernel.Abstractions;

namespace ACommerce.AccountingKernel.Persistence;

/// <summary>
/// بوابة الحفظ الموحدة - النقطة التي يمر عبرها كل حفظ كيانات.
/// تربط المحرك المحاسبي بـ SharedKernel Repository.
///
/// المطور يسجل كيانات في القيد عبر EntityOperations،
/// والمحرك يمررها عبر هذه البوابة إلى IBaseAsyncRepository.
/// </summary>
public interface IPersistenceGateway
{
    /// <summary>
    /// إضافة كيان جديد عبر Repository
    /// </summary>
    Task<object> AddAsync(Type entityType, object entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// تحديث كيان عبر Repository
    /// </summary>
    Task UpdateAsync(Type entityType, object entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// تحديث جزئي عبر Repository
    /// </summary>
    Task PartialUpdateAsync(Type entityType, Guid id, Dictionary<string, object> updates, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف منطقي
    /// </summary>
    Task SoftDeleteAsync(Type entityType, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// حذف نهائي
    /// </summary>
    Task HardDeleteAsync(Type entityType, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// استعادة محذوف
    /// </summary>
    Task RestoreAsync(Type entityType, Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// بوابة فارغة - عندما لا يُراد حفظ كيانات (إشعارات، مصادقة، إلخ)
/// </summary>
public class NullPersistenceGateway : IPersistenceGateway
{
    public Task<object> AddAsync(Type entityType, object entity, CancellationToken ct = default) => Task.FromResult(entity);
    public Task UpdateAsync(Type entityType, object entity, CancellationToken ct = default) => Task.CompletedTask;
    public Task PartialUpdateAsync(Type entityType, Guid id, Dictionary<string, object> updates, CancellationToken ct = default) => Task.CompletedTask;
    public Task SoftDeleteAsync(Type entityType, Guid id, CancellationToken ct = default) => Task.CompletedTask;
    public Task HardDeleteAsync(Type entityType, Guid id, CancellationToken ct = default) => Task.CompletedTask;
    public Task RestoreAsync(Type entityType, Guid id, CancellationToken ct = default) => Task.CompletedTask;
}
