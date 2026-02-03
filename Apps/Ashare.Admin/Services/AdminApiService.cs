using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ashare.Admin.Services;

public class AdminApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AdminAuthStateProvider _authStateProvider;
    private readonly ILogger<AdminApiService> _logger;
    private readonly string _baseUrl;

    public AdminApiService(
        IConfiguration configuration, 
        AdminAuthStateProvider authStateProvider,
        ILogger<AdminApiService> logger)
    {
        _configuration = configuration;
        _authStateProvider = authStateProvider;
        _logger = logger;
        _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ashare-api-130415035604.me-central2.run.app";
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authStateProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync("/api/admin/dashboard/stats");
            
            if (response.IsSuccessStatusCode)
            {
                var stats = await response.Content.ReadFromJsonAsync<ApiDashboardStats>();
                if (stats != null)
                {
                    return new DashboardStats
                    {
                        TotalUsers = stats.TotalUsers,
                        TotalListings = stats.TotalListings,
                        TotalOrders = stats.TotalOrders,
                        TotalRevenue = stats.TotalRevenue,
                        ActiveVendors = stats.ActiveVendors,
                        PendingListings = stats.PendingListings,
                        NewUsersThisMonth = stats.NewUsersThisMonth,
                        OrdersThisMonth = stats.OrdersThisMonth
                    };
                }
            }
            
            _logger.LogWarning("Failed to fetch dashboard stats from API: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats from API");
        }

        return GetMockDashboardStats();
    }

    public async Task<List<RecentOrder>> GetRecentOrdersAsync(int count = 10)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"/api/admin/orders?pageSize={count}&orderBy=CreatedAt&ascending=false");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiOrderListResponse>();
                if (result?.Items != null)
                {
                    return result.Items.Select(o => new RecentOrder
                    {
                        Id = o.Id?.ToString() ?? "",
                        CustomerName = o.CustomerName ?? "غير معروف",
                        Amount = o.Total,
                        Status = TranslateOrderStatus(o.Status),
                        Date = o.CreatedAt
                    }).ToList();
                }
            }
            
            _logger.LogWarning("Failed to fetch recent orders from API: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent orders from API");
        }

        return GetMockRecentOrders();
    }

    public async Task<List<ChartDataPoint>> GetSalesChartDataAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync("/api/admin/dashboard/revenue?days=180");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ApiRevenueItem>>();
                if (result != null && result.Any())
                {
                    return result.Select(r => new ChartDataPoint
                    {
                        Label = r.Period ?? "",
                        Value = (double)r.Revenue
                    }).ToList();
                }
            }
            
            _logger.LogWarning("Failed to fetch sales chart data from API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales chart data from API");
        }

        return GetMockSalesChartData();
    }

    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync($"/api/admin/reports/users?page={page}&pageSize={pageSize}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiUserActivityReport>();
                if (result?.TopActiveUsers != null)
                {
                    return result.TopActiveUsers.Select(u => new UserDto
                    {
                        Id = u.UserId?.ToString() ?? "",
                        Name = u.UserName ?? "غير معروف",
                        Email = u.Email ?? "",
                        Phone = u.Phone ?? "",
                        Role = "عميل",
                        Status = "نشط",
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                }
                _logger.LogWarning("Users API returned null data, using mock data");
            }
            else
            {
                _logger.LogWarning("Failed to fetch users from API: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from API");
        }

        return GetMockUsers();
    }

    public async Task<List<ListingDto>> GetListingsAsync(int page = 1, int pageSize = 20, string? status = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"/api/admin/listings?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(status))
            {
                url += $"&status={status}";
            }
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiListingListResponse>();
                if (result?.Items != null)
                {
                    return result.Items.Select(l => new ListingDto
                    {
                        Id = l.Id?.ToString() ?? "",
                        Title = l.Title ?? "بدون عنوان",
                        VendorName = l.VendorName ?? "غير معروف",
                        Price = l.Price,
                        Status = TranslateListingStatus(l.Status),
                        Category = l.CategoryName ?? "غير مصنف",
                        CreatedAt = l.CreatedAt
                    }).ToList();
                }
            }
            
            _logger.LogWarning("Failed to fetch listings from API: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching listings from API");
        }

        return GetMockListings();
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int page = 1, int pageSize = 20, string? status = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"/api/admin/orders?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(status))
            {
                url += $"&status={status}";
            }
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiOrderListResponse>();
                if (result?.Items != null)
                {
                    return result.Items.Select(o => new OrderDto
                    {
                        Id = o.Id?.ToString() ?? "",
                        CustomerName = o.CustomerName ?? "غير معروف",
                        VendorName = o.VendorName ?? "غير معروف",
                        Total = o.Total,
                        Status = TranslateOrderStatus(o.Status),
                        PaymentStatus = TranslatePaymentStatus(o.PaymentStatus),
                        CreatedAt = o.CreatedAt
                    }).ToList();
                }
            }
            
            _logger.LogWarning("Failed to fetch orders from API: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders from API");
        }

        return GetMockOrders();
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = "/api/admin/reports/sales";
            var queryParams = new List<string>();

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SalesReportDto>();
                if (result != null)
                {
                    return result;
                }
                _logger.LogWarning("Sales report API returned null data, using mock data");
            }
            else
            {
                _logger.LogWarning("Failed to fetch sales report from API: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales report from API");
        }

        return new SalesReportDto
        {
            TotalRevenue = 125000m,
            TotalOrders = 432,
            AverageOrderValue = 289.35m
        };
    }

    // ============================================================================
    // Notification Methods
    // ============================================================================

    public async Task<NotificationStatsDto> GetNotificationStatsAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync("/api/admin/notifications/stats");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NotificationStatsDto>();
                if (result != null)
                {
                    return result;
                }
            }

            _logger.LogWarning("Failed to fetch notification stats: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notification stats");
        }

        return new NotificationStatsDto { TotalUsers = 0, ActiveUsers = 0, UsersWithDevices = 0 };
    }

    public async Task<List<NotificationUserDto>?> GetNotificationUsersAsync(string? search = null, int page = 1, int pageSize = 20)
    {
        try
        {
            await SetAuthHeaderAsync();
            var url = $"/api/admin/notifications/users?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<NotificationUsersResponse>();
                return result?.Items;
            }

            _logger.LogWarning("Failed to fetch notification users: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notification users");
        }

        return new List<NotificationUserDto>();
    }

    public async Task<SendNotificationResultDto?> SendNotificationAsync(SendNotificationRequestDto request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/api/admin/notifications/send", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SendNotificationResultDto>();
            }

            _logger.LogWarning("Failed to send notification: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
        }

        return null;
    }

    public async Task<SendNotificationResultDto?> BroadcastNotificationAsync(BroadcastNotificationRequestDto request)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/api/admin/notifications/broadcast", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SendNotificationResultDto>();
            }

            _logger.LogWarning("Failed to broadcast notification: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification");
        }

        return null;
    }

    // ============================================================================
    // Subscription Methods - Using real API endpoints
    // ============================================================================

    /// <summary>
    /// Get all subscription plans from the real API
    /// Endpoint: GET /api/subscriptions/plans
    /// </summary>
    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync("/api/subscriptions/plans");

            if (response.IsSuccessStatusCode)
            {
                var apiPlans = await response.Content.ReadFromJsonAsync<List<ApiSubscriptionPlanDto>>();
                if (apiPlans != null)
                {
                    return apiPlans.Select(MapToSubscriptionPlanDto).ToList();
                }
            }

            _logger.LogWarning("Failed to fetch subscription plans: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching subscription plans");
        }

        return new List<SubscriptionPlanDto>();
    }

    /// <summary>
    /// Get users (profiles) with their subscription info
    /// Endpoint: POST /api/profiles/search
    /// </summary>
    public async Task<List<UserWithSubscriptionDto>> GetUsersWithSubscriptionsAsync(int page = 1, int pageSize = 20)
    {
        try
        {
            await SetAuthHeaderAsync();

            // Search profiles using the standard search endpoint
            var searchRequest = new ProfileSearchRequest
            {
                PageNumber = page,
                PageSize = pageSize,
                SortBy = "CreatedAt",
                SortDescending = true
            };

            var response = await _httpClient.PostAsJsonAsync("/api/profiles/search", searchRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiProfileDto>>();
                if (result?.Items != null)
                {
                    var users = new List<UserWithSubscriptionDto>();

                    foreach (var profile in result.Items)
                    {
                        var user = MapToUserWithSubscriptionDto(profile);

                        // Try to get subscription info for vendor profiles
                        if (profile.Type == "Vendor" && Guid.TryParse(profile.Id, out var vendorId))
                        {
                            try
                            {
                                var subResponse = await _httpClient.GetAsync($"/api/subscriptions/vendor/{vendorId}");
                                if (subResponse.IsSuccessStatusCode)
                                {
                                    var subscription = await subResponse.Content.ReadFromJsonAsync<ApiSubscriptionDto>();
                                    if (subscription != null)
                                    {
                                        user.PlanName = subscription.Plan?.Name;
                                        user.MaxListings = subscription.MaxListings;
                                        user.CurrentListings = subscription.CurrentListingsCount;
                                        user.SubscriptionStatus = subscription.Status;
                                        user.SubscriptionEndDate = subscription.CurrentPeriodEnd;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to get subscription for vendor {VendorId}", vendorId);
                            }
                        }

                        users.Add(user);
                    }

                    return users;
                }
            }

            _logger.LogWarning("Failed to fetch users: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users with subscriptions");
        }

        return new List<UserWithSubscriptionDto>();
    }

    /// <summary>
    /// Get user details with subscription history
    /// Endpoints: GET /api/profiles/{id}, GET /api/subscriptions/vendor/{vendorId}
    /// </summary>
    public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
    {
        try
        {
            await SetAuthHeaderAsync();

            // Get profile details
            var profileResponse = await _httpClient.GetAsync($"/api/profiles/{userId}");
            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch profile: {StatusCode}", profileResponse.StatusCode);
                return null;
            }

            var profile = await profileResponse.Content.ReadFromJsonAsync<ApiProfileDto>();
            if (profile == null) return null;

            var userDetails = new UserDetailsDto
            {
                Id = profile.Id ?? userId,
                Name = profile.FullName ?? profile.BusinessName ?? "غير معروف",
                Email = profile.Email ?? "",
                Phone = profile.PhoneNumber ?? "",
                Role = profile.Type == "Vendor" ? "بائع" : "عميل",
                Status = profile.IsActive ? "نشط" : "غير نشط",
                CreatedAt = profile.CreatedAt
            };

            // Get subscription info for vendors
            if (profile.Type == "Vendor" && Guid.TryParse(profile.Id, out var vendorId))
            {
                try
                {
                    var subResponse = await _httpClient.GetAsync($"/api/subscriptions/vendor/{vendorId}");
                    if (subResponse.IsSuccessStatusCode)
                    {
                        var subscription = await subResponse.Content.ReadFromJsonAsync<ApiSubscriptionDto>();
                        if (subscription != null)
                        {
                            userDetails.CurrentSubscription = new UserSubscriptionDto
                            {
                                Id = subscription.Id?.ToString() ?? "",
                                UserId = userId,
                                PlanId = subscription.PlanId?.ToString() ?? "",
                                PlanName = subscription.Plan?.Name ?? "",
                                MaxListings = subscription.MaxListings,
                                CurrentListings = subscription.CurrentListingsCount,
                                Status = subscription.Status ?? "",
                                StartDate = subscription.StartDate,
                                EndDate = subscription.CurrentPeriodEnd,
                                CreatedAt = subscription.StartDate
                            };
                        }
                    }

                    // Get usage stats
                    var usageResponse = await _httpClient.GetAsync($"/api/subscriptions/vendor/{vendorId}/usage");
                    if (usageResponse.IsSuccessStatusCode)
                    {
                        var usage = await usageResponse.Content.ReadFromJsonAsync<ApiVendorUsageStatsDto>();
                        if (usage != null && userDetails.CurrentSubscription != null)
                        {
                            userDetails.CurrentSubscription.CurrentListings = usage.ListingsUsed;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get subscription details for vendor {VendorId}", vendorId);
                }
            }

            return userDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user details for {UserId}", userId);
        }

        return null;
    }

    /// <summary>
    /// Create subscription plan
    /// Endpoint: POST /api/subscriptions/plans (Admin only)
    /// </summary>
    public async Task<bool> CreateSubscriptionPlanAsync(SubscriptionPlanDto plan)
    {
        try
        {
            await SetAuthHeaderAsync();
            var createDto = MapToCreateSubscriptionPlanDto(plan);
            var response = await _httpClient.PostAsJsonAsync("/api/subscriptions/plans", createDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan");
            return false;
        }
    }

    /// <summary>
    /// Update subscription plan
    /// Endpoint: PUT /api/subscriptions/plans/{planId} (Admin only)
    /// </summary>
    public async Task<bool> UpdateSubscriptionPlanAsync(string planId, SubscriptionPlanDto plan)
    {
        try
        {
            await SetAuthHeaderAsync();
            var updateDto = MapToUpdateSubscriptionPlanDto(plan);
            var response = await _httpClient.PutAsJsonAsync($"/api/subscriptions/plans/{planId}", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription plan {PlanId}", planId);
            return false;
        }
    }

    /// <summary>
    /// Delete subscription plan
    /// Endpoint: DELETE /api/subscriptions/plans/{planId} (Admin only)
    /// </summary>
    public async Task<bool> DeleteSubscriptionPlanAsync(string planId)
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.DeleteAsync($"/api/subscriptions/plans/{planId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription plan {PlanId}", planId);
            return false;
        }
    }

    // ============================================================================
    // Mapping Helpers
    // ============================================================================

    private static SubscriptionPlanDto MapToSubscriptionPlanDto(ApiSubscriptionPlanDto api) => new()
    {
        Id = api.Id?.ToString() ?? "",
        Name = api.NameEn ?? api.Name ?? "",
        NameAr = api.Name ?? "",
        Description = api.Description,
        Price = api.MonthlyPrice,
        DurationDays = 30, // Default to monthly
        MaxListings = api.MaxListings,
        PlanType = api.IsDefault ? "default" : "standard",
        IsActive = true,
        SubscribersCount = 0, // Would need separate API call
        CreatedAt = DateTime.UtcNow
    };

    private static UserWithSubscriptionDto MapToUserWithSubscriptionDto(ApiProfileDto profile) => new()
    {
        Id = profile.Id ?? "",
        Name = profile.FullName ?? profile.BusinessName ?? "غير معروف",
        Email = profile.Email ?? "",
        Phone = profile.PhoneNumber ?? "",
        Role = profile.Type == "Vendor" ? "بائع" : "عميل",
        Status = profile.IsActive ? "نشط" : "غير نشط",
        CreatedAt = profile.CreatedAt
    };

    private static object MapToCreateSubscriptionPlanDto(SubscriptionPlanDto plan) => new
    {
        Name = plan.NameAr,
        NameEn = plan.Name,
        Slug = plan.Name.ToLower().Replace(" ", "-"),
        Description = plan.Description,
        MonthlyPrice = plan.Price,
        MaxListings = plan.MaxListings,
        MaxImagesPerListing = 10,
        ListingDurationDays = plan.DurationDays,
        IsDefault = false,
        IsRecommended = false
    };

    private static object MapToUpdateSubscriptionPlanDto(SubscriptionPlanDto plan) => new
    {
        Id = Guid.Parse(plan.Id),
        Name = plan.NameAr,
        NameEn = plan.Name,
        Slug = plan.Name.ToLower().Replace(" ", "-"),
        Description = plan.Description,
        MonthlyPrice = plan.Price,
        MaxListings = plan.MaxListings,
        MaxImagesPerListing = 10,
        ListingDurationDays = plan.DurationDays,
        IsActive = plan.IsActive,
        IsDefault = false,
        IsRecommended = false
    };

    private string TranslateOrderStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "pending" => "معلق",
            "processing" => "قيد المعالجة",
            "confirmed" => "مؤكد",
            "shipped" => "تم الشحن",
            "delivered" => "تم التوصيل",
            "completed" => "مكتمل",
            "cancelled" => "ملغي",
            "refunded" => "مسترد",
            _ => status ?? "غير معروف"
        };
    }

    private string TranslatePaymentStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "pending" => "معلق",
            "paid" => "مدفوع",
            "failed" => "فشل",
            "refunded" => "مسترد",
            _ => status ?? "غير معروف"
        };
    }

    private string TranslateListingStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "draft" => "مسودة",
            "pending" => "قيد المراجعة",
            "approved" => "نشط",
            "rejected" => "مرفوض",
            "suspended" => "معلق",
            "active" => "نشط",
            _ => status ?? "غير معروف"
        };
    }

    private DashboardStats GetMockDashboardStats() => new()
    {
        TotalUsers = 1250,
        TotalListings = 845,
        TotalOrders = 432,
        TotalRevenue = 125000m,
        ActiveVendors = 56,
        PendingListings = 23,
        NewUsersThisMonth = 89,
        OrdersThisMonth = 78
    };

    private List<RecentOrder> GetMockRecentOrders() => new()
    {
        new() { Id = "ORD-001", CustomerName = "أحمد محمد", Amount = 450m, Status = "مكتمل", Date = DateTime.Now.AddDays(-1) },
        new() { Id = "ORD-002", CustomerName = "سارة العلي", Amount = 320m, Status = "قيد التوصيل", Date = DateTime.Now.AddDays(-1) },
        new() { Id = "ORD-003", CustomerName = "خالد السعيد", Amount = 890m, Status = "جديد", Date = DateTime.Now },
    };

    private List<ChartDataPoint> GetMockSalesChartData() => new()
    {
        new() { Label = "يناير", Value = 15000 },
        new() { Label = "فبراير", Value = 18000 },
        new() { Label = "مارس", Value = 22000 },
        new() { Label = "أبريل", Value = 19000 },
        new() { Label = "مايو", Value = 25000 },
        new() { Label = "يونيو", Value = 28000 },
    };

    private List<UserDto> GetMockUsers() => new()
    {
        new() { Id = "1", Name = "أحمد محمد", Email = "ahmed@example.com", Phone = "0501234567", Role = "عميل", Status = "نشط", CreatedAt = DateTime.Now.AddMonths(-3) },
        new() { Id = "2", Name = "سارة العلي", Email = "sara@example.com", Phone = "0559876543", Role = "بائع", Status = "نشط", CreatedAt = DateTime.Now.AddMonths(-2) },
    };

    private List<ListingDto> GetMockListings() => new()
    {
        new() { Id = "1", Title = "مساحة عمل مشتركة - الرياض", VendorName = "شركة المكاتب", Price = 500m, Status = "نشط", Category = "مساحات عمل", CreatedAt = DateTime.Now.AddDays(-5) },
        new() { Id = "2", Title = "قاعة اجتماعات VIP", VendorName = "مركز الأعمال", Price = 300m, Status = "معلق", Category = "قاعات", CreatedAt = DateTime.Now.AddDays(-3) },
    };

    private List<OrderDto> GetMockOrders() => new()
    {
        new() { Id = "ORD-001", CustomerName = "أحمد محمد", VendorName = "شركة المكاتب", Total = 450m, Status = "مكتمل", PaymentStatus = "مدفوع", CreatedAt = DateTime.Now.AddDays(-1) },
        new() { Id = "ORD-002", CustomerName = "سارة العلي", VendorName = "مركز الأعمال", Total = 320m, Status = "قيد التوصيل", PaymentStatus = "مدفوع", CreatedAt = DateTime.Now.AddDays(-1) },
    };
}

public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int TotalListings { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveVendors { get; set; }
    public int PendingListings { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int OrdersThisMonth { get; set; }
}

public class RecentOrder
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ListingDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SalesReportDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class ApiDashboardStats
{
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }
    
    [JsonPropertyName("totalListings")]
    public int TotalListings { get; set; }
    
    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }
    
    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }
    
    [JsonPropertyName("activeVendors")]
    public int ActiveVendors { get; set; }
    
    [JsonPropertyName("pendingListings")]
    public int PendingListings { get; set; }
    
    [JsonPropertyName("newUsersThisMonth")]
    public int NewUsersThisMonth { get; set; }
    
    [JsonPropertyName("ordersThisMonth")]
    public int OrdersThisMonth { get; set; }
}

public class ApiRevenueItem
{
    [JsonPropertyName("period")]
    public string? Period { get; set; }
    
    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }
}

public class ApiOrderListResponse
{
    [JsonPropertyName("items")]
    public List<ApiOrderItem>? Items { get; set; }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}

public class ApiOrderItem
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }
    
    [JsonPropertyName("customerName")]
    public string? CustomerName { get; set; }
    
    [JsonPropertyName("vendorName")]
    public string? VendorName { get; set; }
    
    [JsonPropertyName("total")]
    public decimal Total { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("paymentStatus")]
    public string? PaymentStatus { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class ApiListingListResponse
{
    [JsonPropertyName("items")]
    public List<ApiListingItem>? Items { get; set; }
    
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}

public class ApiListingItem
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }
    
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("vendorName")]
    public string? VendorName { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class ApiUserActivityReport
{
    [JsonPropertyName("topActiveUsers")]
    public List<ApiUserItem>? TopActiveUsers { get; set; }
}

public class ApiUserItem
{
    [JsonPropertyName("userId")]
    public Guid? UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}

// ============================================================================
// Notification DTOs & Methods
// ============================================================================

public class NotificationUserDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("deviceCount")]
    public int DeviceCount { get; set; }

    [JsonPropertyName("hasDevices")]
    public bool HasDevices { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class NotificationStatsDto
{
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }

    [JsonPropertyName("activeUsers")]
    public int ActiveUsers { get; set; }

    [JsonPropertyName("usersWithDevices")]
    public int UsersWithDevices { get; set; }

    [JsonPropertyName("totalActiveDevices")]
    public int TotalActiveDevices { get; set; }
}

public class SendNotificationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public string? ActionUrl { get; set; }
    public List<string> UserIds { get; set; } = new();
}

public class BroadcastNotificationRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public string? ActionUrl { get; set; }
}

public class SendNotificationResultDto
{
    [JsonPropertyName("totalSent")]
    public int TotalSent { get; set; }

    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }

    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }
}

public class NotificationUsersResponse
{
    [JsonPropertyName("items")]
    public List<NotificationUserDto>? Items { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

// ============================================================================
// Subscription DTOs
// ============================================================================

public class SubscriptionPlanDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nameAr")]
    public string NameAr { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("durationDays")]
    public int DurationDays { get; set; }

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }

    [JsonPropertyName("planType")]
    public string PlanType { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("subscribersCount")]
    public int SubscribersCount { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class UserSubscriptionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("planId")]
    public string PlanId { get; set; } = string.Empty;

    [JsonPropertyName("planName")]
    public string PlanName { get; set; } = string.Empty;

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }

    [JsonPropertyName("currentListings")]
    public int CurrentListings { get; set; }

    [JsonPropertyName("remainingListings")]
    public int RemainingListings => MaxListings == -1 ? -1 : MaxListings - CurrentListings;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

public class UserWithSubscriptionDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    // Subscription Info
    [JsonPropertyName("planName")]
    public string? PlanName { get; set; }

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }

    [JsonPropertyName("currentListings")]
    public int CurrentListings { get; set; }

    [JsonPropertyName("remainingListings")]
    public int RemainingListings => MaxListings == -1 ? -1 : MaxListings - CurrentListings;

    [JsonPropertyName("subscriptionStatus")]
    public string? SubscriptionStatus { get; set; }

    [JsonPropertyName("subscriptionEndDate")]
    public DateTime? SubscriptionEndDate { get; set; }
}

public class UserDetailsDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("currentSubscription")]
    public UserSubscriptionDto? CurrentSubscription { get; set; }

    [JsonPropertyName("subscriptionHistory")]
    public List<UserSubscriptionDto> SubscriptionHistory { get; set; } = new();

    [JsonPropertyName("listings")]
    public List<ListingDto> Listings { get; set; } = new();
}

public class UsersWithSubscriptionsResponse
{
    [JsonPropertyName("items")]
    public List<UserWithSubscriptionDto>? Items { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}

// ============================================================================
// API Response DTOs - Match real API structure
// ============================================================================

/// <summary>
/// DTO matching the real SubscriptionPlanDto from the API
/// </summary>
public class ApiSubscriptionPlanDto
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameEn")]
    public string? NameEn { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("descriptionEn")]
    public string? DescriptionEn { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }

    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    [JsonPropertyName("isRecommended")]
    public bool IsRecommended { get; set; }

    [JsonPropertyName("monthlyPrice")]
    public decimal MonthlyPrice { get; set; }

    [JsonPropertyName("quarterlyPrice")]
    public decimal? QuarterlyPrice { get; set; }

    [JsonPropertyName("semiAnnualPrice")]
    public decimal? SemiAnnualPrice { get; set; }

    [JsonPropertyName("annualPrice")]
    public decimal? AnnualPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("trialDays")]
    public int TrialDays { get; set; }

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }

    [JsonPropertyName("maxImagesPerListing")]
    public int MaxImagesPerListing { get; set; }

    [JsonPropertyName("maxFeaturedListings")]
    public int MaxFeaturedListings { get; set; }

    [JsonPropertyName("storageLimitMB")]
    public int StorageLimitMB { get; set; }

    [JsonPropertyName("listingDurationDays")]
    public int ListingDurationDays { get; set; }

    [JsonPropertyName("commissionPercentage")]
    public decimal CommissionPercentage { get; set; }

    [JsonPropertyName("hasVerifiedBadge")]
    public bool HasVerifiedBadge { get; set; }
}

/// <summary>
/// DTO matching the real ProfileResponseDto from the API
/// </summary>
public class ApiProfileDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("businessName")]
    public string? BusinessName { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO matching the real SubscriptionDto from the API
/// </summary>
public class ApiSubscriptionDto
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("vendorId")]
    public Guid? VendorId { get; set; }

    [JsonPropertyName("planId")]
    public Guid? PlanId { get; set; }

    [JsonPropertyName("plan")]
    public ApiSubscriptionPlanSummaryDto? Plan { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("billingCycle")]
    public string? BillingCycle { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("currentPeriodEnd")]
    public DateTime CurrentPeriodEnd { get; set; }

    [JsonPropertyName("trialEndDate")]
    public DateTime? TrialEndDate { get; set; }

    [JsonPropertyName("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "SAR";

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }

    [JsonPropertyName("currentListingsCount")]
    public int CurrentListingsCount { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("daysRemaining")]
    public int DaysRemaining { get; set; }
}

public class ApiSubscriptionPlanSummaryDto
{
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameEn")]
    public string? NameEn { get; set; }

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("monthlyPrice")]
    public decimal MonthlyPrice { get; set; }

    [JsonPropertyName("maxListings")]
    public int MaxListings { get; set; }
}

/// <summary>
/// DTO matching the real VendorUsageStatsDto from the API
/// </summary>
public class ApiVendorUsageStatsDto
{
    [JsonPropertyName("vendorId")]
    public Guid VendorId { get; set; }

    [JsonPropertyName("subscriptionId")]
    public Guid SubscriptionId { get; set; }

    [JsonPropertyName("planName")]
    public string? PlanName { get; set; }

    [JsonPropertyName("listingsUsed")]
    public int ListingsUsed { get; set; }

    [JsonPropertyName("listingsLimit")]
    public int ListingsLimit { get; set; }

    [JsonPropertyName("listingsUsagePercentage")]
    public decimal ListingsUsagePercentage { get; set; }

    [JsonPropertyName("featuredUsed")]
    public int FeaturedUsed { get; set; }

    [JsonPropertyName("featuredLimit")]
    public int FeaturedLimit { get; set; }

    [JsonPropertyName("storageUsedMB")]
    public decimal StorageUsedMB { get; set; }

    [JsonPropertyName("storageLimitMB")]
    public int StorageLimitMB { get; set; }
}

/// <summary>
/// Generic paged result DTO
/// </summary>
public class ApiPagedResult<T>
{
    [JsonPropertyName("items")]
    public List<T>? Items { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

/// <summary>
/// Profile search request matching the SmartSearchRequest
/// </summary>
public class ProfileSearchRequest
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;

    [JsonPropertyName("sortBy")]
    public string? SortBy { get; set; }

    [JsonPropertyName("sortDescending")]
    public bool SortDescending { get; set; }

    [JsonPropertyName("filters")]
    public List<SearchFilter>? Filters { get; set; }
}

public class SearchFilter
{
    [JsonPropertyName("propertyName")]
    public string PropertyName { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "Equals";
}
