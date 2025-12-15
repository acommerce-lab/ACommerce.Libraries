using ACommerce.Orders.Enums;
using ACommerce.Admin.Reports.DTOs;
using ACommerce.Orders;
using ACommerce.Orders.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Reports.Queries;

public record GetSalesReportQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IBaseAsyncRepository<Order> _orderRepository;

    public GetSalesReportQueryHandler(IBaseAsyncRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var orders = await _orderRepository.GetAll()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Where(o => o.Status == OrderStatus.Completed)
            .ToListAsync(cancellationToken);

        var totalRevenue = orders.Sum(o => o.Total);
        var totalOrders = orders.Count;
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var dailyBreakdown = orders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesByDayDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            DailyBreakdown = dailyBreakdown,
            CategoryBreakdown = new List<SalesByCategoryDto>()
        };
    }
}
