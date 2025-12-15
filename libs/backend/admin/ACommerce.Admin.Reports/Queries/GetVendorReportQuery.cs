using ACommerce.Orders.Enums;
using ACommerce.Admin.Reports.DTOs;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Orders;
using ACommerce.Orders.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Vendors.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Reports.Queries;

public record GetVendorReportQuery(DateTime? StartDate, DateTime? EndDate, int TopCount = 10) : IRequest<VendorReportDto>;

public class GetVendorReportQueryHandler : IRequestHandler<GetVendorReportQuery, VendorReportDto>
{
    private readonly IBaseAsyncRepository<Vendor> _vendorRepository;
    private readonly IBaseAsyncRepository<ProductListing> _listingRepository;
    private readonly IBaseAsyncRepository<Order> _orderRepository;

    public GetVendorReportQueryHandler(
        IBaseAsyncRepository<Vendor> vendorRepository,
        IBaseAsyncRepository<ProductListing> listingRepository,
        IBaseAsyncRepository<Order> orderRepository)
    {
        _vendorRepository = vendorRepository;
        _listingRepository = listingRepository;
        _orderRepository = orderRepository;
    }

    public async Task<VendorReportDto> Handle(GetVendorReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var vendors = await _vendorRepository.GetAll().ToListAsync(cancellationToken);
        var totalVendors = vendors.Count;
        var activeVendors = vendors.Count(v => v.IsApproved);

        var orders = await _orderRepository.GetAll()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .Where(o => o.Status == OrderStatus.Delivered)
            .ToListAsync(cancellationToken);

        var totalVendorRevenue = orders.Sum(o => o.Total);

        var listings = await _listingRepository.GetAll().ToListAsync(cancellationToken);

        var topVendors = vendors
            .Select(v =>
            {
                var vendorListings = listings.Where(l => l.VendorId == v.Id).ToList();
                var vendorListingIds = vendorListings.Select(l => l.Id).ToHashSet();
                var vendorOrders = orders.Where(o => o.Items.Any(i => vendorListingIds.Contains(i.ProductListingId ?? Guid.Empty))).ToList();

                return new TopVendorReportDto
                {
                    VendorId = v.Id,
                    VendorName = v.Name,
                    ListingCount = vendorListings.Count,
                    OrderCount = vendorOrders.Count,
                    Revenue = vendorOrders.Sum(o => o.Total),
                    Rating = 0
                };
            })
            .OrderByDescending(v => v.Revenue)
            .Take(request.TopCount)
            .ToList();

        return new VendorReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalVendors = totalVendors,
            ActiveVendors = activeVendors,
            TotalVendorRevenue = totalVendorRevenue,
            TopVendors = topVendors
        };
    }
}
