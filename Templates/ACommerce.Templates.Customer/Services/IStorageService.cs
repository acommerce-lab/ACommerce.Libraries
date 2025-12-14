namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Storage service interface for cross-platform storage
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Get a value from storage
    /// </summary>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Set a value in storage
    /// </summary>
    Task SetAsync(string key, string value);

    /// <summary>
    /// Remove a value from storage
    /// </summary>
    Task RemoveAsync(string key);
}

/// <summary>
/// In-memory storage implementation (fallback for testing)
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
}
