using ACommerce.Admin.Listings.DTOs;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Listings.Queries;

public record GetAdminListingsQuery(AdminListingFilterDto Filter) : IRequest<AdminListingListDto>;

public class GetAdminListingsQueryHandler : IRequestHandler<GetAdminListingsQuery, AdminListingListDto>
{
    private readonly IBaseAsyncRepository<ProductListing> _repository;

    public GetAdminListingsQueryHandler(IBaseAsyncRepository<ProductListing> repository)
    {
        _repository = repository;
    }

    public async Task<AdminListingListDto> Handle(GetAdminListingsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _repository.GetAll();

        if (filter.Status.HasValue)
            query = query.Where(l => l.Status == filter.Status.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(l => l.IsActive == filter.IsActive.Value);

        if (filter.VendorId.HasValue)
            query = query.Where(l => l.VendorId == filter.VendorId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(l => l.CategoryId == filter.CategoryId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(l => l.CreatedAt <= filter.EndDate.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => l.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(l => l.Title.Contains(filter.SearchTerm) || (l.Description != null && l.Description.Contains(filter.SearchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        query = filter.OrderBy?.ToLower() switch
        {
            "price" => filter.Ascending ? query.OrderBy(l => l.Price) : query.OrderByDescending(l => l.Price),
            "viewcount" => filter.Ascending ? query.OrderBy(l => l.ViewCount) : query.OrderByDescending(l => l.ViewCount),
            "title" => filter.Ascending ? query.OrderBy(l => l.Title) : query.OrderByDescending(l => l.Title),
            _ => filter.Ascending ? query.OrderBy(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(l => new AdminListingItemDto
            {
                Id = l.Id,
                Title = l.Title,
                VendorId = l.VendorId,
                CategoryId = l.CategoryId,
                Status = l.Status,
                IsActive = l.IsActive,
                Price = l.Price,
                ViewCount = l.ViewCount,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new AdminListingListDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }
}
