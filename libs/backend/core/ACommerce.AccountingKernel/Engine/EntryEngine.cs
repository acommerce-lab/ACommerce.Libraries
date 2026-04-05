using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.AccountingKernel.Engine;

/// <summary>
/// محرك تنفيذ القيود.
/// يسأل EntryEngineOptions: هل أوثّق؟ هل أحفظ كيانات؟ هل أنشر أحداث؟
/// لا يعتمد على أي مزود محدد (لا EF Core ولا MediatR مباشرة).
/// </summary>
public class EntryEngine
{
    private readonly IServiceProvider _services;
    private readonly IEntryStore _entryStore;
    private readonly IPersistenceGateway _persistenceGateway;
    private readonly IEventPublisher _eventPublisher;
    private readonly EntryEngineOptions _options;
    private readonly ILogger<EntryEngine> _logger;

    public EntryEngine(
        IServiceProvider services,
        IEntryStore entryStore,
        IPersistenceGateway persistenceGateway,
        IEventPublisher eventPublisher,
        IOptions<EntryEngineOptions> options,
        ILogger<EntryEngine> logger)
    {
        _services = services;
        _entryStore = entryStore;
        _persistenceGateway = persistenceGateway;
        _eventPublisher = eventPublisher;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// تنفيذ قيد كامل مع كل قيوده الفرعية
    /// </summary>
    public async Task<EntryResult> ExecuteAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var context = new EntryContext(entry, _services, cancellationToken);
        var result = new EntryResult { EntryId = entry.Id, EntryType = entry.EntryType, Context = context };

        _logger.LogInformation("[{EntryType}] {Id}: Starting", entry.EntryType, entry.Id);

        try
        {
            // 1. فحص التوازن (إن كان مطلوباً)
            if (_options.EnforceBalance && entry.Legs.Any(l => l.Value != 0) && !entry.IsBalanced())
            {
                return Fail(entry, result, "Entry is not balanced: debits must equal credits");
            }

            // 2. التحقق المخصص
            if (entry.ValidateFunc != null && !await entry.ValidateFunc(context))
            {
                // جمع أخطاء التحقق من Context
                context.TryGet<string>("_validationError", out var err);
                if (context.TryGet<List<string>>("_validationErrors", out var errors) && errors != null)
                    result.ValidationErrors = errors;
                else if (err != null)
                    result.ValidationErrors.Add(err);
                return await FailWithCallback(entry, context, result, err ?? "Validation failed");
            }

            entry.Status = EntryStatus.Executing;

            // 3. حفظ كيانات عبر البوابة (إن كان مفعّلاً)
            if (_options.EnableEntityPersistence)
                await ExecuteEntityOperations(entry, context, cancellationToken);

            // 4. المنطق المخصص
            if (entry.ExecuteFunc != null)
                await entry.ExecuteFunc(context);

            // 5. تحديث حالة الأطراف
            foreach (var leg in entry.Legs)
                if (leg.Status == LegStatus.Pending)
                    leg.Status = LegStatus.Completed;

            // 6. القيود الفرعية
            var subResults = new List<EntryResult>();
            foreach (var subEntry in entry.SubEntries.Cast<Entry>())
            {
                var subContext = new EntryContext(subEntry, _services, cancellationToken) { ParentEntry = entry };
                foreach (var item in context.Items)
                    subContext.Items.TryAdd(item.Key, item.Value);

                subResults.Add(await ExecuteAsync(subEntry, cancellationToken));
            }
            result.SubResults = subResults;

            // 7. التحقق اللاحق
            if (entry.PostValidateFunc != null)
                await entry.PostValidateFunc(context);

            // 8. تحديد الحالة النهائية
            DetermineStatus(entry, result, subResults);

            // 9. توثيق (إن كان مفعّلاً)
            if (_options.EnableAudit)
                await _entryStore.SaveEntryAsync(entry, cancellationToken);

            // 10. نشر الأحداث (إن كان مفعّلاً)
            if (_options.EnableEvents && result.Success)
                await PublishEvents(entry, context, cancellationToken);

            // 11. callback
            if (result.Success && entry.OnCompletedFunc != null)
                await entry.OnCompletedFunc(context);
            else if (!result.Success && entry.OnFailedFunc != null)
                await entry.OnFailedFunc(context);

            _logger.LogInformation("[{EntryType}] {Id}: {Status}", entry.EntryType, entry.Id, entry.Status);
        }
        catch (Exception ex)
        {
            entry.Status = EntryStatus.Failed;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "[{EntryType}] {Id}: Exception", entry.EntryType, entry.Id);

            if (entry.OnErrorFunc != null)
                await entry.OnErrorFunc(context, ex);

            if (_options.EnableAudit)
                await _entryStore.SaveEntryAsync(entry, cancellationToken);
        }

        return result;
    }

    private async Task ExecuteEntityOperations(Entry entry, EntryContext context, CancellationToken ct)
    {
        foreach (var op in entry.EntityOperations)
        {
            switch (op.Type)
            {
                case EntityOperationType.Add:
                    var saved = await _persistenceGateway.AddAsync(op.EntityType, op.EntityFactory(context), ct);
                    context.Set($"_entity_{op.EntityType.Name}", saved);
                    break;
                case EntityOperationType.Update:
                    await _persistenceGateway.UpdateAsync(op.EntityType, op.EntityFactory(context), ct);
                    break;
                case EntityOperationType.PartialUpdate:
                    await _persistenceGateway.PartialUpdateAsync(op.EntityType, op.EntityIdResolver!(context), op.UpdateFields!(context), ct);
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

    private async Task PublishEvents(Entry entry, EntryContext context, CancellationToken ct)
    {
        foreach (var factory in entry.EventFactories)
        {
            var evt = factory(context);
            await _eventPublisher.PublishAsync(evt, ct);
        }
    }

    private static void DetermineStatus(Entry entry, EntryResult result, List<EntryResult> subResults)
    {
        if (subResults.Count == 0 || subResults.All(r => r.Success))
        {
            entry.Status = EntryStatus.Completed;
            entry.CompletedAt = DateTime.UtcNow;
            result.Success = true;
        }
        else if (subResults.Any(r => r.Success))
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
    }

    private EntryResult Fail(Entry entry, EntryResult result, string error)
    {
        entry.Status = EntryStatus.Failed;
        result.Success = false;
        result.ErrorMessage = error;
        _logger.LogWarning("[{EntryType}] {Id}: {Error}", entry.EntryType, entry.Id, error);
        return result;
    }

    private async Task<EntryResult> FailWithCallback(Entry entry, EntryContext ctx, EntryResult result, string error)
    {
        Fail(entry, result, error);
        if (entry.OnFailedFunc != null) await entry.OnFailedFunc(ctx);
        if (_options.EnableAudit) await _entryStore.SaveEntryAsync(entry, ctx.CancellationToken);
        return result;
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
    public List<string> ValidationErrors { get; set; } = new();
    public List<EntryResult> SubResults { get; set; } = new();

    /// <summary>
    /// سياق التنفيذ - يحمل البيانات الناتجة عن التنفيذ.
    /// يستخدمه المتحكم لاستخراج النتائج.
    /// </summary>
    public EntryContext? Context { get; internal set; }
}
