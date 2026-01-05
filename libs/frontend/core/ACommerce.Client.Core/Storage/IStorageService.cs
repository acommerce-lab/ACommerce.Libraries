namespace ACommerce.Client.Core.Storage;

/// <summary>
/// واجهة التخزين عبر المنصات
/// يتم تنفيذها بشكل مختلف حسب المنصة:
/// - MAUI: SecureStorage + Preferences
/// - Web: localStorage via JSInterop
/// - Server: In-Memory (للاختبار)
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// الحصول على قيمة من التخزين
    /// </summary>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// حفظ قيمة في التخزين
    /// </summary>
    Task SetAsync(string key, string value);

    /// <summary>
    /// حذف قيمة من التخزين
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// هل المفتاح موجود؟
    /// </summary>
    Task<bool> ContainsKeyAsync(string key);
}

/// <summary>
/// تنفيذ التخزين في الذاكرة (للاختبار والـ fallback)
/// </summary>
public class InMemoryStorageService : IStorageService
{
    private readonly Dictionary<string, string> _storage = new();

    public Task<string?> GetAsync(string key)
    {
        _storage.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(string key, string value)
    {
        _storage[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _storage.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string key)
    {
        return Task.FromResult(_storage.ContainsKey(key));
    }
}
