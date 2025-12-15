using ACommerce.Orders.Enums;
using ACommerce.Admin.Dashboard.DTOs;
using ACommerce.Orders.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Dashboard.Queries;

public record GetRevenueByPeriodQuery(int Days = 30) : IRequest<List<RevenueByPeriodDto>>;

public class GetRevenueByPeriodQueryHandler : IRequestHandler<GetRevenueByPeriodQuery, List<RevenueByPeriodDto>>
{
    private readonly DbContext _dbContext;

    public GetRevenueByPeriodQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RevenueByPeriodDto>> Handle(GetRevenueByPeriodQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-request.Days);
        
        var orders = await _dbContext.Set<Order>()
            .Where(o => !o.IsDeleted && o.Status == OrderStatus.Completed && o.CreatedAt >= startDate)
            .ToListAsync(cancellationToken);

        var grouped = orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueByPeriodDto
            {
                Period = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Period)
            .ToList();

        return grouped;
    }
}
