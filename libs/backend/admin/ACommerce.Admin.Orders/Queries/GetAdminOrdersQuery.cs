using ACommerce.Admin.Orders.DTOs;
using ACommerce.Orders.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Orders.Queries;

public record GetAdminOrdersQuery(AdminOrderFilterDto Filter) : IRequest<AdminOrderListDto>;

public class GetAdminOrdersQueryHandler : IRequestHandler<GetAdminOrdersQuery, AdminOrderListDto>
{
    private readonly IBaseAsyncRepository<Order> _repository;

    public GetAdminOrdersQueryHandler(IBaseAsyncRepository<Order> repository)
    {
        _repository = repository;
    }

    public async Task<AdminOrderListDto> Handle(GetAdminOrdersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _repository.GetAll();

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);

        if (filter.VendorId.HasValue)
            query = query.Where(o => o.VendorId == filter.VendorId.Value);

        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId.Value);

        if (filter.MinTotal.HasValue)
            query = query.Where(o => o.Total >= filter.MinTotal.Value);

        if (filter.MaxTotal.HasValue)
            query = query.Where(o => o.Total <= filter.MaxTotal.Value);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(o => o.Id.ToString().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        query = filter.OrderBy?.ToLower() switch
        {
            "total" => filter.Ascending ? query.OrderBy(o => o.Total) : query.OrderByDescending(o => o.Total),
            "status" => filter.Ascending ? query.OrderBy(o => o.Status) : query.OrderByDescending(o => o.Status),
            _ => filter.Ascending ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new AdminOrderItemDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                VendorId = o.VendorId,
                Status = o.Status,
                Total = o.Total,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new AdminOrderListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }
}
