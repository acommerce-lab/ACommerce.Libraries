using System.Text.Json;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة لحفظ بيانات العرض المعلق أثناء عملية الدفع
/// </summary>
public class PendingListingService
{
    private const string StorageKey = "pending_listing";
    private readonly IStorageService _storage;

    public PendingListingService(IStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// حفظ بيانات العرض المعلق
    /// </summary>
    public async Task SavePendingListingAsync(PendingListingData data)
    {
        var json = JsonSerializer.Serialize(data);
        await _storage.SetAsync(StorageKey, json);
    }

    /// <summary>
    /// استرجاع بيانات العرض المعلق
    /// </summary>
    public async Task<PendingListingData?> GetPendingListingAsync()
    {
        var json = await _storage.GetAsync(StorageKey);
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<PendingListingData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// مسح بيانات العرض المعلق
    /// </summary>
    public async Task ClearPendingListingAsync()
    {
        await _storage.RemoveAsync(StorageKey);
    }

    /// <summary>
    /// التحقق من وجود عرض معلق
    /// </summary>
    public async Task<bool> HasPendingListingAsync()
    {
        var data = await GetPendingListingAsync();
        return data != null;
    }
}

/// <summary>
/// بيانات العرض المعلق
/// </summary>
public class PendingListingData
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Images { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
