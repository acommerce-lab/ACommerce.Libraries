using System.Net.Http.Json;
using Order.Shared.Models;

namespace Order.Shared.Services;

/// <summary>
/// خدمة API لتطبيق اوردر
/// </summary>
public class OrderApiService
{
    private readonly HttpClient _httpClient;

    public OrderApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Auth

    /// <summary>
    /// إرسال كود التحقق
    /// </summary>
    public async Task<SendCodeResponse?> SendVerificationCodeAsync(string phoneNumber)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/send-code", new { PhoneNumber = phoneNumber });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SendCodeResponse>();
    }

    /// <summary>
    /// التحقق من الكود وتسجيل الدخول
    /// </summary>
    public async Task<LoginResponse?> VerifyCodeAsync(string phoneNumber, string code)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/verify-code", new { PhoneNumber = phoneNumber, Code = code });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    /// <summary>
    /// الحصول على المستخدم الحالي
    /// </summary>
    public async Task<UserProfileDto?> GetCurrentUserAsync()
    {
        return await _httpClient.GetFromJsonAsync<UserProfileDto>("api/auth/me");
    }

    #endregion

    #region Offers

    /// <summary>
    /// الحصول على العروض
    /// </summary>
    public async Task<OffersResponse?> GetOffersAsync(
        double? latitude = null,
        double? longitude = null,
        double radiusKm = 10,
        Guid? categoryId = null,
        Guid? vendorId = null,
        string? search = null,
        int page = 1,
        int pageSize = 20)
    {
        var queryParams = new List<string>();

        if (latitude.HasValue)
            queryParams.Add($"latitude={latitude}");
        if (longitude.HasValue)
            queryParams.Add($"longitude={longitude}");
        if (radiusKm != 10)
            queryParams.Add($"radiusKm={radiusKm}");
        if (categoryId.HasValue)
            queryParams.Add($"categoryId={categoryId}");
        if (vendorId.HasValue)
            queryParams.Add($"vendorId={vendorId}");
        if (!string.IsNullOrEmpty(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");

        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var url = $"api/offers?{string.Join("&", queryParams)}";
        return await _httpClient.GetFromJsonAsync<OffersResponse>(url);
    }

    /// <summary>
    /// الحصول على تفاصيل عرض
    /// </summary>
    public async Task<OfferDto?> GetOfferAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<OfferDto>($"api/offers/{id}");
    }

    /// <summary>
    /// الحصول على الفئات
    /// </summary>
    public async Task<List<CategoryDto>?> GetCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/productcategories");
    }

    #endregion

    #region Orders

    /// <summary>
    /// إنشاء طلب جديد
    /// </summary>
    public async Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/customer/orders", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
    }

    /// <summary>
    /// الحصول على طلباتي
    /// </summary>
    public async Task<OrdersResponse?> GetMyOrdersAsync(int page = 1, int pageSize = 20)
    {
        return await _httpClient.GetFromJsonAsync<OrdersResponse>($"api/customer/orders?page={page}&pageSize={pageSize}");
    }

    /// <summary>
    /// الحصول على تفاصيل طلب
    /// </summary>
    public async Task<OrderDto?> GetOrderAsync(Guid orderId)
    {
        return await _httpClient.GetFromJsonAsync<OrderDto>($"api/customer/orders/{orderId}");
    }

    /// <summary>
    /// تحديث موقع التوصيل
    /// </summary>
    public async Task UpdateDeliveryLocationAsync(Guid orderId, double latitude, double longitude, string? description = null)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/customer/orders/{orderId}/location", new
        {
            Latitude = latitude,
            Longitude = longitude,
            LocationDescription = description
        });
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// إلغاء طلب
    /// </summary>
    public async Task CancelOrderAsync(Guid orderId)
    {
        var response = await _httpClient.PostAsync($"api/customer/orders/{orderId}/cancel", null);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Vendors

    /// <summary>
    /// الحصول على تفاصيل متجر
    /// </summary>
    public async Task<VendorDto?> GetVendorAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<VendorDto>($"api/vendors/{id}");
    }

    #endregion
}

#region Response Models

public class SendCodeResponse
{
    public string? Message { get; set; }
    public string? Phone { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string? DebugCode { get; set; } // فقط في التطوير
}

public class LoginResponse
{
    public string? Token { get; set; }
    public UserProfileDto? Profile { get; set; }
    public string? Message { get; set; }
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class CreateOrderResponse
{
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Message { get; set; }
}

#endregion
