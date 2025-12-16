using ACommerce.Catalog.Listings.Enums;

namespace ACommerce.Admin.Listings.DTOs;

public class AdminListingFilterDto
{
    public ListingStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string OrderBy { get; set; } = "CreatedAt";
    public bool Ascending { get; set; } = false;
}

public class AdminListingListDto
{
    public List<AdminListingItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AdminListingItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public Guid? CategoryId { get; set; }
    public ListingStatus Status { get; set; }
    public bool IsActive { get; set; }
    public decimal Price { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
