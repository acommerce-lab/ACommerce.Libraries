using ACommerce.Client.Core.Storage;

namespace com.order.app.Services;

/// <summary>
/// تنفيذ التخزين لتطبيق MAUI باستخدام SecureStorage و Preferences
/// </summary>
public class MauiStorageService : IStorageService
{
    // المفاتيح التي تحتاج تخزين آمن (مثل tokens)
    private static readonly HashSet<string> SecureKeys = new()
    {
        "auth_token",
        "refresh_token",
        "user_data"
    };

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            if (SecureKeys.Contains(key))
            {
                return await SecureStorage.Default.GetAsync(key);
            }
            return Preferences.Default.Get<string?>(key, null);
        }
        catch (Exception)
        {
            // Fallback to Preferences if SecureStorage fails
            return Preferences.Default.Get<string?>(key, null);
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            if (SecureKeys.Contains(key))
            {
                await SecureStorage.Default.SetAsync(key, value);
            }
            else
            {
                Preferences.Default.Set(key, value);
            }
        }
        catch (Exception)
        {
            // Fallback to Preferences if SecureStorage fails
            Preferences.Default.Set(key, value);
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            if (SecureKeys.Contains(key))
            {
                SecureStorage.Default.Remove(key);
            }
            else
            {
                Preferences.Default.Remove(key);
            }
        }
        catch (Exception)
        {
            Preferences.Default.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            if (SecureKeys.Contains(key))
            {
                // SecureStorage doesn't have ContainsKey, try to get value
                var task = SecureStorage.Default.GetAsync(key);
                task.Wait();
                return Task.FromResult(task.Result != null);
            }
            return Task.FromResult(Preferences.Default.ContainsKey(key));
        }
        catch (Exception)
        {
            return Task.FromResult(Preferences.Default.ContainsKey(key));
        }
    }
}
