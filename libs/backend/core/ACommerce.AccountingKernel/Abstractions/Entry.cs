namespace ACommerce.AccountingKernel.Abstractions;

/// <summary>
/// التطبيق الافتراضي للقيد العملياتي
/// </summary>
public class Entry : IEntry
{
    private readonly List<ILeg> _legs = new();
    private readonly List<IEntry> _subEntries = new();

    public Guid Id { get; } = Guid.NewGuid();
    public string EntryType { get; }
    public string? Description { get; set; }
    public IReadOnlyList<ILeg> Legs => _legs.AsReadOnly();
    public IReadOnlyList<IEntry> SubEntries => _subEntries.AsReadOnly();
    public Guid? ParentEntryId { get; internal set; }
    public EntryStatus Status { get; internal set; } = EntryStatus.Created;
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; internal set; }
    public Dictionary<string, object> Metadata { get; } = new();

    /// <summary>
    /// دوال التحقق والتنفيذ - يحددها المطور عبر Builder
    /// </summary>
    internal Func<EntryContext, Task<bool>>? ValidateFunc { get; set; }
    internal Func<EntryContext, Task>? ExecuteFunc { get; set; }
    internal Func<EntryContext, Task>? PostValidateFunc { get; set; }
    internal Func<EntryContext, Task>? OnCompletedFunc { get; set; }
    internal Func<EntryContext, Task>? OnFailedFunc { get; set; }
    internal Func<EntryContext, Exception, Task>? OnErrorFunc { get; set; }

    /// <summary>
    /// مصانع الأحداث التي تُطلق عند الاكتمال
    /// </summary>
    internal List<Func<EntryContext, object>> EventFactories { get; } = new();

    /// <summary>
    /// عمليات حفظ الكيانات (يُنفذها المحرك عبر IPersistenceGateway إن كان مفعّلاً)
    /// </summary>
    internal List<EntityOperation> EntityOperations { get; } = new();

    public Entry(string entryType)
    {
        EntryType = entryType;
    }

    internal void AddLeg(ILeg leg) => _legs.Add(leg);
    internal void AddSubEntry(Entry subEntry)
    {
        subEntry.ParentEntryId = Id;
        _subEntries.Add(subEntry);
    }

    /// <summary>
    /// هل القيد متوازن؟ (مجموع المدين = مجموع الدائن)
    /// </summary>
    public bool IsBalanced()
    {
        var debitSum = _legs.Where(l => l.Direction == LegDirection.Debit).Sum(l => l.Value);
        var creditSum = _legs.Where(l => l.Direction == LegDirection.Credit).Sum(l => l.Value);
        return debitSum == creditSum;
    }
}

/// <summary>
/// عملية على كيان يُنفذها المحرك عبر IPersistenceGateway
/// </summary>
public class EntityOperation
{
    public EntityOperationType Type { get; set; }
    public Type EntityType { get; set; } = default!;
    public Func<EntryContext, object> EntityFactory { get; set; } = default!;
    public Func<EntryContext, Guid>? EntityIdResolver { get; set; }
    public Func<EntryContext, Dictionary<string, object>>? UpdateFields { get; set; }
}

public enum EntityOperationType
{
    Add,
    Update,
    PartialUpdate,
    SoftDelete,
    HardDelete,
    Restore
}
