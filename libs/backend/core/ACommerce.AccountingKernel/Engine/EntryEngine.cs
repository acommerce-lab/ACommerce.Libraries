using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.AccountingKernel.Engine;

/// <summary>
/// محرك تنفيذ القيود - يدير دورة الحياة الكاملة:
/// Validate → Execute EntityOps → Execute Logic → PostValidate → Persist → Events
///
/// هذا المحرك لا يهتم بنوع العملية (مالية، إشعار، دردشة، مصادقة).
/// يهتم فقط بـ: التوازن + دورة الحياة + التوثيق.
/// </summary>
public class EntryEngine
{
    private readonly IServiceProvider _services;
    private readonly IEntryStore _entryStore;
    private readonly IPersistenceGateway _persistenceGateway;
    private readonly ILogger<EntryEngine> _logger;

    public EntryEngine(
        IServiceProvider services,
        IEntryStore entryStore,
        IPersistenceGateway persistenceGateway,
        ILogger<EntryEngine> logger)
    {
        _services = services;
        _entryStore = entryStore;
        _persistenceGateway = persistenceGateway;
        _logger = logger;
    }

    /// <summary>
    /// تنفيذ قيد كامل مع كل قيوده الفرعية
    /// </summary>
    public async Task<EntryResult> ExecuteAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var context = new EntryContext(entry, _services, cancellationToken);
        var result = new EntryResult { EntryId = entry.Id, EntryType = entry.EntryType };

        _logger.LogInformation("Entry [{EntryType}] {EntryId}: Starting execution", entry.EntryType, entry.Id);

        try
        {
            // 1. التحقق من التوازن (إذا كان هناك أطراف بقيم)
            if (entry.Legs.Any(l => l.Value != 0) && !entry.IsBalanced())
            {
                entry.Status = EntryStatus.Failed;
                result.Success = false;
                result.ErrorMessage = "Entry is not balanced: sum of debits must equal sum of credits";
                _logger.LogWarning("Entry [{EntryType}] {EntryId}: Balance check failed", entry.EntryType, entry.Id);

                await AuditIfNeeded(entry, cancellationToken);
                return result;
            }

            // 2. التحقق المخصص (Validate)
            if (entry.ValidateFunc != null)
            {
                var isValid = await entry.ValidateFunc(context);
                if (!isValid)
                {
                    entry.Status = EntryStatus.Failed;
                    result.Success = false;
                    result.ErrorMessage = context.TryGet<string>("_validationError", out var err) ? err : "Validation failed";
                    _logger.LogWarning("Entry [{EntryType}] {EntryId}: Validation failed: {Error}", entry.EntryType, entry.Id, result.ErrorMessage);

                    if (entry.OnFailedFunc != null)
                        await entry.OnFailedFunc(context);

                    await AuditIfNeeded(entry, cancellationToken);
                    return result;
                }
            }

            entry.Status = EntryStatus.Validated;

            // 3. تنفيذ عمليات الكيانات عبر SharedKernel (إن وُجدت)
            entry.Status = EntryStatus.Executing;
            await ExecuteEntityOperations(entry, context, cancellationToken);

            // 4. تنفيذ المنطق المخصص (Execute)
            if (entry.ExecuteFunc != null)
            {
                await entry.ExecuteFunc(context);
            }

            // 5. تحديث حالة الأطراف
            foreach (var leg in entry.Legs.Cast<Leg>())
            {
                if (leg.Status == LegStatus.Pending)
                    leg.Status = LegStatus.Completed;
            }

            // 6. تنفيذ القيود الفرعية
            var subResults = new List<EntryResult>();
            foreach (var subEntry in entry.SubEntries.Cast<Entry>())
            {
                var subContext = new EntryContext(subEntry, _services, cancellationToken)
                {
                    ParentEntry = entry
                };

                // نقل البيانات المشتركة من الأب للفرع
                foreach (var item in context.Items)
                    subContext.Items.TryAdd(item.Key, item.Value);

                var subResult = await ExecuteAsync(subEntry, cancellationToken);
                subResults.Add(subResult);
            }

            result.SubResults = subResults;

            // 7. التحقق اللاحق (PostValidate)
            if (entry.PostValidateFunc != null)
            {
                await entry.PostValidateFunc(context);
            }

            // 8. تحديد الحالة النهائية
            var allSubsCompleted = subResults.All(r => r.Success);
            var anySubCompleted = subResults.Any(r => r.Success);

            if (subResults.Count == 0 || allSubsCompleted)
            {
                entry.Status = EntryStatus.Completed;
                entry.CompletedAt = DateTime.UtcNow;
                result.Success = true;
            }
            else if (anySubCompleted)
            {
                entry.Status = EntryStatus.PartiallyCompleted;
                entry.CompletedAt = DateTime.UtcNow;
                result.Success = true;
                result.IsPartial = true;
            }
            else
            {
                entry.Status = EntryStatus.Failed;
                result.Success = false;
                result.ErrorMessage = "All sub-entries failed";
            }

            // 9. توثيق القيد (إن طُلب)
            await AuditIfNeeded(entry, cancellationToken);

            // 10. إطلاق الأحداث عبر MediatR
            if (result.Success)
            {
                await PublishEvents(entry, context);

                if (entry.OnCompletedFunc != null)
                    await entry.OnCompletedFunc(context);
            }
            else if (entry.OnFailedFunc != null)
            {
                await entry.OnFailedFunc(context);
            }

            _logger.LogInformation(
                "Entry [{EntryType}] {EntryId}: Completed with status {Status}",
                entry.EntryType, entry.Id, entry.Status);
        }
        catch (Exception ex)
        {
            entry.Status = EntryStatus.Failed;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;

            _logger.LogError(ex, "Entry [{EntryType}] {EntryId}: Execution failed", entry.EntryType, entry.Id);

            if (entry.OnErrorFunc != null)
                await entry.OnErrorFunc(context, ex);

            await AuditIfNeeded(entry, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// تنفيذ عمليات الكيانات عبر بوابة SharedKernel
    /// </summary>
    private async Task ExecuteEntityOperations(Entry entry, EntryContext context, CancellationToken ct)
    {
        if (entry.PersistenceMode is not (EntryPersistenceMode.ExecuteAndPersist or EntryPersistenceMode.Full))
            return;

        foreach (var op in entry.EntityOperations)
        {
            switch (op.Type)
            {
                case EntityOperationType.Add:
                    var entity = op.EntityFactory(context);
                    var saved = await _persistenceGateway.AddAsync(op.EntityType, entity, ct);
                    context.Set($"_entity_{op.EntityType.Name}", saved);
                    break;

                case EntityOperationType.Update:
                    var toUpdate = op.EntityFactory(context);
                    await _persistenceGateway.UpdateAsync(op.EntityType, toUpdate, ct);
                    break;

                case EntityOperationType.PartialUpdate:
                    var id = op.EntityIdResolver!(context);
                    var fields = op.UpdateFields!(context);
                    await _persistenceGateway.PartialUpdateAsync(op.EntityType, id, fields, ct);
                    break;

                case EntityOperationType.SoftDelete:
                    await _persistenceGateway.SoftDeleteAsync(op.EntityType, op.EntityIdResolver!(context), ct);
                    break;

                case EntityOperationType.HardDelete:
                    await _persistenceGateway.HardDeleteAsync(op.EntityType, op.EntityIdResolver!(context), ct);
                    break;

                case EntityOperationType.Restore:
                    await _persistenceGateway.RestoreAsync(op.EntityType, op.EntityIdResolver!(context), ct);
                    break;
            }
        }
    }

    private async Task AuditIfNeeded(Entry entry, CancellationToken ct)
    {
        if (entry.PersistenceMode is EntryPersistenceMode.ExecuteAndAudit or EntryPersistenceMode.Full)
        {
            await _entryStore.SaveEntryAsync(entry, ct);
        }
    }

    private async Task PublishEvents(Entry entry, EntryContext context)
    {
        var mediator = _services.GetService<IMediator>();
        if (mediator == null) return;

        foreach (var factory in entry.EventFactories)
        {
            var evt = factory(context);
            if (evt is INotification notification)
            {
                await mediator.Publish(notification, context.CancellationToken);
            }
        }
    }
}

/// <summary>
/// نتيجة تنفيذ قيد
/// </summary>
public class EntryResult
{
    public Guid EntryId { get; set; }
    public string EntryType { get; set; } = default!;
    public bool Success { get; set; }
    public bool IsPartial { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public List<EntryResult> SubResults { get; set; } = new();
}
