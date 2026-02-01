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

    #region Extended Auth Methods

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return new LoginResult
                {
                    Success = true,
                    Token = result?.Token,
                    UserId = result?.Profile?.Id.ToString()
                };
            }
            return new LoginResult { Success = false, ErrorMessage = "فشل تسجيل الدخول" };
        }
        catch
        {
            return new LoginResult { Success = false, ErrorMessage = "خطأ في الاتصال" };
        }
    }

    public async Task<UserStats> GetUserStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserStats>("api/customer/stats") ?? new UserStats();
        }
        catch
        {
            return new UserStats();
        }
    }

    #endregion

    #region Extended Orders Methods

    public async Task<OrdersListResponse> GetOrdersAsync(int page = 1)
    {
        try
        {
            var result = await GetMyOrdersAsync(page);
            return new OrdersListResponse
            {
                Items = result?.Orders?.Select(o => new OrderListItemDto
                {
                    Id = o.Id.ToString(),
                    Number = o.OrderNumber ?? "",
                    Status = o.Status ?? "",
                    Total = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    VendorName = o.VendorName,
                    VendorLogo = o.VendorLogoUrl,
                    ItemsCount = o.Items?.Count ?? 0,
                    FirstItemName = o.Items?.FirstOrDefault()?.Name,
                    FirstItemImage = o.Items?.FirstOrDefault()?.ImageUrl,
                    CanReorder = true,
                    CanCancel = o.Status == "pending",
                    CanTrack = o.Status == "out_for_delivery",
                    CanReview = o.Status == "delivered",
                    CanChat = true
                }).ToList() ?? new List<OrderListItemDto>(),
                HasMore = (result?.TotalCount ?? 0) > page * 20
            };
        }
        catch
        {
            return new OrdersListResponse { Items = new List<OrderListItemDto>() };
        }
    }

    public async Task<List<OrderListItemDto>> GetRecentOrdersAsync()
    {
        var result = await GetOrdersAsync(1);
        return result.Items.Take(10).ToList();
    }

    public async Task ReorderAsync(string orderId)
    {
        await _httpClient.PostAsync($"api/customer/orders/{orderId}/reorder", null);
    }

    #endregion

    #region Tickets Methods

    public async Task<List<TicketDto>> GetTicketsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<TicketDto>>("api/customer/tickets") ?? new List<TicketDto>();
        }
        catch
        {
            return new List<TicketDto>();
        }
    }

    public async Task<TicketDetailDto> GetTicketAsync(string ticketId)
    {
        return await _httpClient.GetFromJsonAsync<TicketDetailDto>($"api/customer/tickets/{ticketId}") ?? new TicketDetailDto();
    }

    public async Task<string> CreateTicketAsync(string category, string subject, string message, string? orderId = null)
    {
        var response = await _httpClient.PostAsJsonAsync("api/customer/tickets", new
        {
            Category = category,
            Subject = subject,
            Message = message,
            LinkedOrderId = orderId
        });
        var result = await response.Content.ReadFromJsonAsync<CreateTicketResponse>();
        return result?.TicketId ?? "";
    }

    public async Task ReplyToTicketAsync(string ticketId, string message)
    {
        await _httpClient.PostAsJsonAsync($"api/customer/tickets/{ticketId}/reply", new { Message = message });
    }

    public async Task ReopenTicketAsync(string ticketId)
    {
        await _httpClient.PostAsync($"api/customer/tickets/{ticketId}/reopen", null);
    }

    #endregion

    #region Notifications Methods

    public async Task<List<NotificationDto>> GetNotificationsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<NotificationDto>>("api/customer/notifications") ?? new List<NotificationDto>();
        }
        catch
        {
            return new List<NotificationDto>();
        }
    }

    public async Task MarkNotificationReadAsync(string notificationId)
    {
        await _httpClient.PostAsync($"api/customer/notifications/{notificationId}/read", null);
    }

    public async Task MarkAllNotificationsReadAsync()
    {
        await _httpClient.PostAsync("api/customer/notifications/read-all", null);
    }

    #endregion

    #region Locations Methods

    public async Task<List<SavedLocationDto>> GetSavedLocationsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<SavedLocationDto>>("api/customer/locations") ?? new List<SavedLocationDto>();
        }
        catch
        {
            return new List<SavedLocationDto>();
        }
    }

    public async Task DeleteLocationAsync(string locationId)
    {
        await _httpClient.DeleteAsync($"api/customer/locations/{locationId}");
    }

    #endregion

    #region Chat Methods

    public async Task<List<ChatDto>> GetChatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ChatDto>>("api/customer/chats") ?? new List<ChatDto>();
        }
        catch
        {
            return new List<ChatDto>();
        }
    }

    public async Task<ChatDetailDto> GetChatAsync(string orderId)
    {
        return await _httpClient.GetFromJsonAsync<ChatDetailDto>($"api/customer/chats/{orderId}") ?? new ChatDetailDto();
    }

    public async Task SendChatMessageAsync(string orderId, string message)
    {
        await _httpClient.PostAsJsonAsync($"api/customer/chats/{orderId}/messages", new { Message = message });
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

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserStats
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int SavedAddresses { get; set; }
}

public class OrdersListResponse
{
    public List<OrderListItemDto> Items { get; set; } = new();
    public bool HasMore { get; set; }
}

public class OrderListItemDto
{
    public string Id { get; set; } = "";
    public string Number { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? VendorName { get; set; }
    public string? VendorLogo { get; set; }
    public int ItemsCount { get; set; }
    public string? FirstItemName { get; set; }
    public string? FirstItemImage { get; set; }
    public bool CanReorder { get; set; }
    public bool CanCancel { get; set; }
    public bool CanTrack { get; set; }
    public bool CanReview { get; set; }
    public bool CanChat { get; set; }
}

public class TicketDto
{
    public string Id { get; set; } = "";
    public string Number { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public string? LastMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UnreadReplies { get; set; }
    public string? LinkedOrderNumber { get; set; }
}

public class TicketDetailDto
{
    public string Id { get; set; } = "";
    public string Number { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? LinkedOrderId { get; set; }
    public string? LinkedOrderNumber { get; set; }
    public List<TicketMessageDto> Messages { get; set; } = new();
}

public class TicketMessageDto
{
    public string Id { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAt { get; set; }
    public bool IsFromSupport { get; set; }
    public string? AgentName { get; set; }
}

public class CreateTicketResponse
{
    public string TicketId { get; set; } = "";
}

public class NotificationDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string>? Data { get; set; }
}

public class SavedLocationDto
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
    public string Address { get; set; } = "";
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Notes { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatDto
{
    public string Id { get; set; } = "";
    public string OrderId { get; set; } = "";
    public string OrderNumber { get; set; } = "";
    public string VendorName { get; set; } = "";
    public string? VendorLogo { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsVendorOnline { get; set; }
}

public class ChatDetailDto
{
    public string Id { get; set; } = "";
    public string OrderId { get; set; } = "";
    public string OrderNumber { get; set; } = "";
    public string OrderStatus { get; set; } = "";
    public string VendorName { get; set; } = "";
    public string? VendorLogo { get; set; }
    public bool IsVendorOnline { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ChatMessageDto
{
    public string Id { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAt { get; set; }
    public bool IsFromMe { get; set; }
    public bool IsRead { get; set; }
    public string Type { get; set; } = "text";
}

#endregion
