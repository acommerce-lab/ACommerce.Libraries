using ACommerce.Orders.Enums;
using ACommerce.Admin.Dashboard.DTOs;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Orders.Entities;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;
using ACommerce.Subscriptions.Entities;
using ACommerce.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly DbContext _dbContext;

    public GetDashboardStatsQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var profiles = _dbContext.Set<Profile>();
        var listings = _dbContext.Set<ProductListing>();
        var orders = _dbContext.Set<Order>();
        var subscriptions = _dbContext.Set<Subscription>();

        var totalUsers = await profiles.CountAsync(p => !p.IsDeleted && p.Type == ProfileType.Customer, cancellationToken);
        var totalVendors = await profiles.CountAsync(p => !p.IsDeleted && p.Type == ProfileType.Vendor, cancellationToken);
        
        var totalListings = await listings.CountAsync(l => !l.IsDeleted, cancellationToken);
        var activeListings = await listings.CountAsync(l => !l.IsDeleted && l.IsActive, cancellationToken);
        
        var totalOrders = await orders.CountAsync(o => !o.IsDeleted, cancellationToken);
        var pendingOrders = await orders.CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Pending, cancellationToken);
        var completedOrders = await orders.CountAsync(o => !o.IsDeleted && o.Status == OrderStatus.Completed, cancellationToken);
        
        var totalRevenue = await orders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed)
            .SumAsync(o => o.Total, cancellationToken);
            
        var monthlyRevenue = await orders
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && o.CreatedAt >= monthStart)
            .SumAsync(o => o.Total, cancellationToken);

        var activeSubscriptions = await subscriptions
            .CountAsync(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active, cancellationToken);

        var newUsersToday = await profiles
            .CountAsync(p => !p.IsDeleted && p.CreatedAt >= today, cancellationToken);
        var newUsersThisWeek = await profiles
            .CountAsync(p => !p.IsDeleted && p.CreatedAt >= weekAgo, cancellationToken);
        var newUsersThisMonth = await profiles
            .CountAsync(p => !p.IsDeleted && p.CreatedAt >= monthAgo, cancellationToken);

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            TotalVendors = totalVendors,
            TotalListings = totalListings,
            ActiveListings = activeListings,
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            CompletedOrders = completedOrders,
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            ActiveSubscriptions = activeSubscriptions,
            NewUsersToday = newUsersToday,
            NewUsersThisWeek = newUsersThisWeek,
            NewUsersThisMonth = newUsersThisMonth
        };
    }
}
