using ACommerce.Client.Core.Storage;

namespace HamtramckHardware.App.Services;

/// <summary>
/// MAUI implementation using SecureStorage with Preferences fallback
/// </summary>
public class MauiStorageService : IStorageService
{
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            // Try SecureStorage first
            var value = await SecureStorage.Default.GetAsync(key);
            if (value != null)
            {
                Console.WriteLine($"[MauiStorageService] GetAsync({key}): found in SecureStorage");
                return value;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiStorageService] GetAsync({key}): SecureStorage failed - {ex.Message}");
        }

        // Fallback to Preferences
        try
        {
            var value = Preferences.Default.Get<string?>(key, null);
            if (value != null)
            {
                Console.WriteLine($"[MauiStorageService] GetAsync({key}): found in Preferences fallback");
            }
            else
            {
                Console.WriteLine($"[MauiStorageService] GetAsync({key}): not found");
            }
            return value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiStorageService] GetAsync({key}): Preferences fallback failed - {ex.Message}");
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.Default.SetAsync(key, value);
            Console.WriteLine($"[MauiStorageService] SetAsync({key}): saved to SecureStorage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiStorageService] SetAsync({key}): SecureStorage failed - {ex.Message}, trying Preferences");
            try
            {
                // Fallback to Preferences if SecureStorage fails
                Preferences.Default.Set(key, value);
                Console.WriteLine($"[MauiStorageService] SetAsync({key}): saved to Preferences fallback");
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"[MauiStorageService] SetAsync({key}): Preferences fallback also failed - {ex2.Message}");
            }
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            SecureStorage.Default.Remove(key);
            Console.WriteLine($"[MauiStorageService] RemoveAsync({key}): removed from SecureStorage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiStorageService] RemoveAsync({key}): SecureStorage failed - {ex.Message}");
        }

        try
        {
            Preferences.Default.Remove(key);
            Console.WriteLine($"[MauiStorageService] RemoveAsync({key}): removed from Preferences");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MauiStorageService] RemoveAsync({key}): Preferences failed - {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        var value = await GetAsync(key);
        return value != null;
    }
}
