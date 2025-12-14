using System.Text.Json;
using ACommerce.Templates.Customer.Services;

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
        try
        {
            // تنظيف التخزين القديم أولاً
            await _storage.RemoveAsync(StorageKey);
            
            // ضغط الصور إذا كانت كبيرة جداً (أكثر من 500KB للصورة الواحدة)
            var compressedImages = new List<string>();
            foreach (var img in data.Images)
            {
                if (img.Length > 500_000)
                {
                    // تخطي الصور الكبيرة جداً - سيتم رفعها لاحقاً
                    compressedImages.Add("LARGE_IMAGE_PLACEHOLDER");
                }
                else
                {
                    compressedImages.Add(img);
                }
            }
            data.Images = compressedImages;
            
            var json = JsonSerializer.Serialize(data);
            await _storage.SetAsync(StorageKey, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PendingListingService] Error saving: {ex.Message}");
            // في حالة الفشل، نحاول حفظ البيانات بدون الصور
            try
            {
                data.Images = new List<string>();
                var json = JsonSerializer.Serialize(data);
                await _storage.SetAsync(StorageKey, json);
            }
            catch
            {
                // تجاهل الخطأ - سيتم التعامل معه لاحقاً
            }
        }
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
