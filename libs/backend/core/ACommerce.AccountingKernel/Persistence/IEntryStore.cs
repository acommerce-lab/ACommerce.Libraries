using ACommerce.AccountingKernel.Abstractions;

namespace ACommerce.AccountingKernel.Persistence;

/// <summary>
/// مخزن القيود - لتوثيق العمليات إن أراد المطور ذلك.
/// التطبيق اختياري: يمكن استخدام InMemory للتطوير أو EF Core للإنتاج.
/// </summary>
public interface IEntryStore
{
    Task SaveEntryAsync(IEntry entry, CancellationToken cancellationToken = default);
    Task UpdateEntryStatusAsync(Guid entryId, EntryStatus status, CancellationToken cancellationToken = default);
    Task<IEntry?> GetEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IEntry>> GetSubEntriesAsync(Guid parentEntryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IEntry>> GetEntriesByTypeAsync(string entryType, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}

/// <summary>
/// تطبيق فارغ - لا يحفظ شيئاً (للوضع ExecuteOnly)
/// </summary>
public class NullEntryStore : IEntryStore
{
    public Task SaveEntryAsync(IEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateEntryStatusAsync(Guid entryId, EntryStatus status, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IEntry?> GetEntryAsync(Guid entryId, CancellationToken cancellationToken = default) => Task.FromResult<IEntry?>(null);
    public Task<IReadOnlyList<IEntry>> GetSubEntriesAsync(Guid parentEntryId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<IEntry>>(Array.Empty<IEntry>());
    public Task<IReadOnlyList<IEntry>> GetEntriesByTypeAsync(string entryType, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<IEntry>>(Array.Empty<IEntry>());
}

/// <summary>
/// تطبيق في الذاكرة - للتطوير والاختبار
/// </summary>
public class InMemoryEntryStore : IEntryStore
{
    private readonly List<IEntry> _entries = new();

    public Task SaveEntryAsync(IEntry entry, CancellationToken cancellationToken = default)
    {
        _entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task UpdateEntryStatusAsync(Guid entryId, EntryStatus status, CancellationToken cancellationToken = default)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is Entry mutable)
            mutable.Status = status;
        return Task.CompletedTask;
    }

    public Task<IEntry?> GetEntryAsync(Guid entryId, CancellationToken cancellationToken = default)
        => Task.FromResult(_entries.FirstOrDefault(e => e.Id == entryId));

    public Task<IReadOnlyList<IEntry>> GetSubEntriesAsync(Guid parentEntryId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<IEntry>>(_entries.Where(e => e.ParentEntryId == parentEntryId).ToList());

    public Task<IReadOnlyList<IEntry>> GetEntriesByTypeAsync(string entryType, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<IEntry>>(_entries.Where(e => e.EntryType == entryType).Skip((page - 1) * pageSize).Take(pageSize).ToList());
}
