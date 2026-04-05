using ACommerce.RealEstate.Api.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ACommerce.RealEstate.Api.Entries;

/// <summary>
/// تجريد تخزين العقارات
/// </summary>
public interface IPropertyStore
{
    Task<Property> AddPropertyAsync(Property property, CancellationToken ct = default);
    Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Property> Items, int TotalCount)> SearchAsync(PropertySearchRequest request, CancellationToken ct = default);
    Task UpdatePropertyAsync(Property property, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<PropertyInquiry> AddInquiryAsync(PropertyInquiry inquiry, CancellationToken ct = default);
}

/// <summary>
/// تطبيق يستخدم IBaseAsyncRepository من SharedKernel - نفس البنية التي يستخدمها عشير.
/// الفرق: أعمدة ثابتة مع فهارس بدلاً من Dynamic Attributes + JOINs.
/// </summary>
public class EfPropertyStore : IPropertyStore
{
    private readonly IBaseAsyncRepository<Property> _propertyRepo;
    private readonly IBaseAsyncRepository<PropertyInquiry> _inquiryRepo;
    private readonly DbContext _db;

    public EfPropertyStore(
        IBaseAsyncRepository<Property> propertyRepo,
        IBaseAsyncRepository<PropertyInquiry> inquiryRepo,
        DbContext db)
    {
        _propertyRepo = propertyRepo;
        _inquiryRepo = inquiryRepo;
        _db = db;
    }

    public async Task<Property> AddPropertyAsync(Property property, CancellationToken ct = default)
        => await _propertyRepo.AddAsync(property, ct);

    public async Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _propertyRepo.GetByIdAsync(id, ct);

    public async Task<(List<Property> Items, int TotalCount)> SearchAsync(PropertySearchRequest request, CancellationToken ct = default)
    {
        // استعلام مباشر على الأعمدة المفهرسة - لا JOINs مع جداول خصائص ديناميكية
        var query = _db.Set<Property>().AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == "active");

        if (!string.IsNullOrEmpty(request.City)) query = query.Where(p => p.City == request.City);
        if (!string.IsNullOrEmpty(request.District)) query = query.Where(p => p.District == request.District);
        if (!string.IsNullOrEmpty(request.Purpose)) query = query.Where(p => p.Purpose == request.Purpose);
        if (!string.IsNullOrEmpty(request.Category)) query = query.Where(p => p.Category == request.Category);
        if (!string.IsNullOrEmpty(request.PropertyType)) query = query.Where(p => p.PropertyType == request.PropertyType);
        if (request.MinPrice.HasValue) query = query.Where(p => p.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(p => p.Price <= request.MaxPrice.Value);
        if (request.MinArea.HasValue) query = query.Where(p => p.Area >= request.MinArea.Value);
        if (request.MinRooms.HasValue) query = query.Where(p => p.Rooms >= request.MinRooms.Value);
        if (request.Furnished.HasValue) query = query.Where(p => p.Furnished == request.Furnished.Value);

        var totalCount = await query.CountAsync(ct);

        query = request.OrderBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "area" => query.OrderByDescending(p => p.Area),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task UpdatePropertyAsync(Property property, CancellationToken ct = default)
        => await _propertyRepo.UpdateAsync(property, ct);

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
        => await _propertyRepo.SoftDeleteAsync(id, ct);

    public async Task<PropertyInquiry> AddInquiryAsync(PropertyInquiry inquiry, CancellationToken ct = default)
        => await _inquiryRepo.AddAsync(inquiry, ct);
}
