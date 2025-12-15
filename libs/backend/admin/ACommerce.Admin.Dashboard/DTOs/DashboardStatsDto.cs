namespace ACommerce.Admin.Dashboard.DTOs;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalVendors { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class RevenueByPeriodDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopVendorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ListingCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}
