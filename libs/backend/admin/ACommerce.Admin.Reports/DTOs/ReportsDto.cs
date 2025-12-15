namespace ACommerce.Admin.Reports.DTOs;

public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<SalesByDayDto> DailyBreakdown { get; set; } = new();
    public List<SalesByCategoryDto> CategoryBreakdown { get; set; } = new();
}

public class SalesByDayDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class SalesByCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}

public class UserActivityReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalNewUsers { get; set; }
    public int TotalActiveUsers { get; set; }
    public List<UsersByDayDto> DailyBreakdown { get; set; } = new();
    public List<UsersByTypeDto> TypeBreakdown { get; set; } = new();
}

public class UsersByDayDto
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
}

public class UsersByTypeDto
{
    public string ProfileType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class VendorReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalVendors { get; set; }
    public int ActiveVendors { get; set; }
    public decimal TotalVendorRevenue { get; set; }
    public List<TopVendorReportDto> TopVendors { get; set; } = new();
}

public class TopVendorReportDto
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int ListingCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal Rating { get; set; }
}

public class ReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? VendorId { get; set; }
    public string? Category { get; set; }
}
