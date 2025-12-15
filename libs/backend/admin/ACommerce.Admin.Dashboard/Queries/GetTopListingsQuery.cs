using ACommerce.Admin.Dashboard.DTOs;
using ACommerce.Catalog.Listings.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Admin.Dashboard.Queries;

public record GetTopListingsQuery(int Count = 10) : IRequest<List<TopListingDto>>;

public class GetTopListingsQueryHandler : IRequestHandler<GetTopListingsQuery, List<TopListingDto>>
{
    private readonly DbContext _dbContext;

    public GetTopListingsQueryHandler(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TopListingDto>> Handle(GetTopListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await _dbContext.Set<ProductListing>()
            .Where(l => !l.IsDeleted && l.IsActive)
            .OrderByDescending(l => l.ViewCount)
            .Take(request.Count)
            .Select(l => new TopListingDto
            {
                Id = l.Id,
                Title = l.Title,
                ViewCount = l.ViewCount,
                OrderCount = 0,
                Revenue = 0
            })
            .ToListAsync(cancellationToken);

        return listings;
    }
}
