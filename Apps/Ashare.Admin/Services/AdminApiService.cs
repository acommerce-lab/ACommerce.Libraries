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
