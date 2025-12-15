namespace Ashare.Admin.Services;

public class AdminApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AdminApiService(IConfiguration configuration)
    {
        _configuration = configuration;
        var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://ashare-api-130415035604.me-central2.run.app";
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        return new DashboardStats
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
    }

    public async Task<List<RecentOrder>> GetRecentOrdersAsync(int count = 10)
    {
        return new List<RecentOrder>
        {
            new() { Id = "ORD-001", CustomerName = "أحمد محمد", Amount = 450m, Status = "مكتمل", Date = DateTime.Now.AddDays(-1) },
            new() { Id = "ORD-002", CustomerName = "سارة العلي", Amount = 320m, Status = "قيد التوصيل", Date = DateTime.Now.AddDays(-1) },
            new() { Id = "ORD-003", CustomerName = "خالد السعيد", Amount = 890m, Status = "جديد", Date = DateTime.Now },
            new() { Id = "ORD-004", CustomerName = "فاطمة الزهراء", Amount = 150m, Status = "مكتمل", Date = DateTime.Now.AddDays(-2) },
            new() { Id = "ORD-005", CustomerName = "محمد العتيبي", Amount = 670m, Status = "قيد المعالجة", Date = DateTime.Now },
        };
    }

    public async Task<List<ChartDataPoint>> GetSalesChartDataAsync()
    {
        return new List<ChartDataPoint>
        {
            new() { Label = "يناير", Value = 15000 },
            new() { Label = "فبراير", Value = 18000 },
            new() { Label = "مارس", Value = 22000 },
            new() { Label = "أبريل", Value = 19000 },
            new() { Label = "مايو", Value = 25000 },
            new() { Label = "يونيو", Value = 28000 },
        };
    }

    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20)
    {
        return new List<UserDto>
        {
            new() { Id = "1", Name = "أحمد محمد", Email = "ahmed@example.com", Phone = "0501234567", Role = "عميل", Status = "نشط", CreatedAt = DateTime.Now.AddMonths(-3) },
            new() { Id = "2", Name = "سارة العلي", Email = "sara@example.com", Phone = "0559876543", Role = "بائع", Status = "نشط", CreatedAt = DateTime.Now.AddMonths(-2) },
            new() { Id = "3", Name = "خالد السعيد", Email = "khaled@example.com", Phone = "0541112233", Role = "عميل", Status = "معلق", CreatedAt = DateTime.Now.AddMonths(-1) },
            new() { Id = "4", Name = "فاطمة الزهراء", Email = "fatima@example.com", Phone = "0523334455", Role = "بائع", Status = "نشط", CreatedAt = DateTime.Now.AddDays(-14) },
            new() { Id = "5", Name = "محمد العتيبي", Email = "mohammed@example.com", Phone = "0567778899", Role = "مدير", Status = "نشط", CreatedAt = DateTime.Now.AddYears(-1) },
        };
    }

    public async Task<List<ListingDto>> GetListingsAsync(int page = 1, int pageSize = 20)
    {
        return new List<ListingDto>
        {
            new() { Id = "1", Title = "مساحة عمل مشتركة - الرياض", VendorName = "شركة المكاتب", Price = 500m, Status = "نشط", Category = "مساحات عمل", CreatedAt = DateTime.Now.AddDays(-5) },
            new() { Id = "2", Title = "قاعة اجتماعات VIP", VendorName = "مركز الأعمال", Price = 300m, Status = "معلق", Category = "قاعات", CreatedAt = DateTime.Now.AddDays(-3) },
            new() { Id = "3", Title = "مكتب خاص - جدة", VendorName = "مكاتب الخليج", Price = 1200m, Status = "نشط", Category = "مكاتب", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = "4", Title = "استوديو تصوير", VendorName = "استوديو الإبداع", Price = 250m, Status = "مرفوض", Category = "استوديوهات", CreatedAt = DateTime.Now.AddDays(-7) },
            new() { Id = "5", Title = "مساحة فعاليات", VendorName = "قاعات المملكة", Price = 2000m, Status = "قيد المراجعة", Category = "فعاليات", CreatedAt = DateTime.Now },
        };
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int page = 1, int pageSize = 20)
    {
        return new List<OrderDto>
        {
            new() { Id = "ORD-001", CustomerName = "أحمد محمد", VendorName = "شركة المكاتب", Total = 450m, Status = "مكتمل", PaymentStatus = "مدفوع", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = "ORD-002", CustomerName = "سارة العلي", VendorName = "مركز الأعمال", Total = 320m, Status = "قيد التوصيل", PaymentStatus = "مدفوع", CreatedAt = DateTime.Now.AddDays(-1) },
            new() { Id = "ORD-003", CustomerName = "خالد السعيد", VendorName = "مكاتب الخليج", Total = 890m, Status = "جديد", PaymentStatus = "معلق", CreatedAt = DateTime.Now },
            new() { Id = "ORD-004", CustomerName = "فاطمة الزهراء", VendorName = "استوديو الإبداع", Total = 150m, Status = "ملغي", PaymentStatus = "مسترد", CreatedAt = DateTime.Now.AddDays(-2) },
            new() { Id = "ORD-005", CustomerName = "محمد العتيبي", VendorName = "قاعات المملكة", Total = 670m, Status = "قيد المعالجة", PaymentStatus = "مدفوع", CreatedAt = DateTime.Now },
        };
    }
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
