using ACommerce.AccountingKernel.Abstractions;

namespace ACommerce.AccountingKernel.Builder;

/// <summary>
/// باني القيود السلس (Fluent Builder).
/// يوفر واجهة موحدة لبناء أي عملية في النظام.
///
/// مثال - إشعار:
///   EntryBuilder.Create("notification.send")
///     .From("System").To("User:123")
///     .WithSubEntry("deliver.email", sub => sub
///       .From("Channel:Email").To("User:123")
///       .Execute(ctx => SendEmail(ctx)))
///     .Build();
///
/// مثال - دردشة:
///   EntryBuilder.Create("chat.message")
///     .From("User:ahmed").To("User:sara")
///     .Execute(ctx => SaveMessage(ctx))
///     .WithSubEntry("confirm.delivered", sub => sub
///       .From("System").To("User:ahmed")
///       .OnEvent&lt;MessageDeliveredEvent&gt;(ctx => new(...)))
///     .Build();
/// </summary>
public class EntryBuilder
{
    private readonly Entry _entry;
    private readonly List<Action<Entry>> _subEntryBuilders = new();

    private EntryBuilder(string entryType)
    {
        _entry = new Entry(entryType);
    }

    /// <summary>
    /// نقطة البدء: إنشاء قيد جديد
    /// </summary>
    public static EntryBuilder Create(string entryType) => new(entryType);

    /// <summary>
    /// وصف القيد
    /// </summary>
    public EntryBuilder Describe(string description)
    {
        _entry.Description = description;
        return this;
    }

    // ========================================================
    // الأطراف (Legs)
    // ========================================================

    /// <summary>
    /// إضافة طرف مدين (المُرسل/المُعطي)
    /// </summary>
    public EntryBuilder From(string party, string resourceType = "Operation", decimal value = 1m)
    {
        _entry.AddLeg(new Leg(party, LegDirection.Debit, resourceType, value));
        return this;
    }

    /// <summary>
    /// إضافة طرف دائن (المُستلم/الحاصل)
    /// </summary>
    public EntryBuilder To(string party, string resourceType = "Operation", decimal value = 1m)
    {
        _entry.AddLeg(new Leg(party, LegDirection.Credit, resourceType, value));
        return this;
    }

    /// <summary>
    /// إضافة طرف مخصص
    /// </summary>
    public EntryBuilder AddLeg(string party, LegDirection direction, string resourceType, decimal value = 1m, Action<Leg>? configure = null)
    {
        var leg = new Leg(party, direction, resourceType, value);
        configure?.Invoke(leg);
        _entry.AddLeg(leg);
        return this;
    }

    // ========================================================
    // دورة الحياة (Lifecycle)
    // ========================================================

    /// <summary>
    /// منطق التحقق المسبق
    /// </summary>
    public EntryBuilder Validate(Func<EntryContext, Task<bool>> validateFunc)
    {
        _entry.ValidateFunc = validateFunc;
        return this;
    }

    /// <summary>
    /// تحقق مسبق متزامن (مختصر)
    /// </summary>
    public EntryBuilder Validate(Func<EntryContext, bool> validateFunc)
    {
        _entry.ValidateFunc = ctx => Task.FromResult(validateFunc(ctx));
        return this;
    }

    /// <summary>
    /// منطق التنفيذ الرئيسي
    /// </summary>
    public EntryBuilder Execute(Func<EntryContext, Task> executeFunc)
    {
        _entry.ExecuteFunc = executeFunc;
        return this;
    }

    /// <summary>
    /// تنفيذ متزامن (مختصر)
    /// </summary>
    public EntryBuilder Execute(Action<EntryContext> executeFunc)
    {
        _entry.ExecuteFunc = ctx => { executeFunc(ctx); return Task.CompletedTask; };
        return this;
    }

    /// <summary>
    /// منطق التحقق اللاحق (بعد التنفيذ وتنفيذ القيود الفرعية)
    /// </summary>
    public EntryBuilder PostValidate(Func<EntryContext, Task> postValidateFunc)
    {
        _entry.PostValidateFunc = postValidateFunc;
        return this;
    }

    /// <summary>
    /// عند اكتمال القيد بنجاح
    /// </summary>
    public EntryBuilder OnCompleted(Func<EntryContext, Task> onCompleted)
    {
        _entry.OnCompletedFunc = onCompleted;
        return this;
    }

    /// <summary>
    /// عند فشل القيد
    /// </summary>
    public EntryBuilder OnFailed(Func<EntryContext, Task> onFailed)
    {
        _entry.OnFailedFunc = onFailed;
        return this;
    }

    /// <summary>
    /// عند حدوث استثناء
    /// </summary>
    public EntryBuilder OnError(Func<EntryContext, Exception, Task> onError)
    {
        _entry.OnErrorFunc = onError;
        return this;
    }

    // ========================================================
    // القيود الفرعية (Sub-Entries)
    // ========================================================

    /// <summary>
    /// إضافة قيد فرعي
    /// </summary>
    public EntryBuilder WithSubEntry(string subEntryType, Action<EntryBuilder> configure)
    {
        var subBuilder = new EntryBuilder(subEntryType);
        configure(subBuilder);
        _entry.AddSubEntry(subBuilder._entry);
        return this;
    }

    /// <summary>
    /// إضافة قيود فرعية ديناميكياً (مثل: قيد لكل قناة إشعار)
    /// </summary>
    public EntryBuilder WithSubEntries<T>(IEnumerable<T> items, Func<T, string> typeSelector, Action<EntryBuilder, T> configure)
    {
        foreach (var item in items)
        {
            var subBuilder = new EntryBuilder(typeSelector(item));
            configure(subBuilder, item);
            _entry.AddSubEntry(subBuilder._entry);
        }
        return this;
    }

    // ========================================================
    // الأحداث (Events via MediatR)
    // ========================================================

    /// <summary>
    /// إطلاق حدث MediatR عند اكتمال القيد
    /// </summary>
    public EntryBuilder PublishEvent<TEvent>(Func<EntryContext, TEvent> eventFactory) where TEvent : class
    {
        _entry.EventFactories.Add(ctx => eventFactory(ctx)!);
        return this;
    }

    // ========================================================
    // عمليات الكيانات عبر SharedKernel
    // ========================================================

    /// <summary>
    /// إضافة كيان جديد عبر Repository عند التنفيذ
    /// </summary>
    public EntryBuilder AddEntity<TEntity>(Func<EntryContext, TEntity> factory) where TEntity : class
    {
        _entry.EntityOperations.Add(new EntityOperation
        {
            Type = EntityOperationType.Add,
            EntityType = typeof(TEntity),
            EntityFactory = ctx => factory(ctx)!
        });
        return this;
    }

    /// <summary>
    /// تحديث جزئي لكيان عبر Repository
    /// </summary>
    public EntryBuilder UpdateEntity<TEntity>(Func<EntryContext, Guid> idResolver, Func<EntryContext, Dictionary<string, object>> fields) where TEntity : class
    {
        _entry.EntityOperations.Add(new EntityOperation
        {
            Type = EntityOperationType.PartialUpdate,
            EntityType = typeof(TEntity),
            EntityIdResolver = idResolver,
            UpdateFields = fields
        });
        return this;
    }

    /// <summary>
    /// حذف منطقي لكيان
    /// </summary>
    public EntryBuilder SoftDeleteEntity<TEntity>(Func<EntryContext, Guid> idResolver) where TEntity : class
    {
        _entry.EntityOperations.Add(new EntityOperation
        {
            Type = EntityOperationType.SoftDelete,
            EntityType = typeof(TEntity),
            EntityIdResolver = idResolver,
            EntityFactory = _ => null!
        });
        return this;
    }

    /// <summary>
    /// بيانات وصفية
    /// </summary>
    public EntryBuilder WithMetadata(string key, object value)
    {
        _entry.Metadata[key] = value;
        return this;
    }

    // ========================================================
    // البناء النهائي
    // ========================================================

    /// <summary>
    /// بناء القيد النهائي
    /// </summary>
    public Entry Build() => _entry;
}
