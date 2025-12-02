using Ashare.Shared.Services;

namespace Ashare.App.Services;

/// <summary>
/// MAUI implementation using SecureStorage
/// </summary>
public class MauiStorageService : IStorageService
{
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch
        {
            // Fallback to Preferences if SecureStorage fails
            Preferences.Default.Set(key, value);
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            SecureStorage.Default.Remove(key);
        }
        catch
        {
            Preferences.Default.Remove(key);
        }
        return Task.CompletedTask;
    }
}
