using ACommerce.RealEstate.Api.Entities;

namespace ACommerce.RealEstate.Api.Entries;

/// <summary>
/// تجريد تخزين العقارات.
/// في التطبيق الحقيقي: يُطبّق عبر EF Core أو أي مزود آخر.
/// هنا نستخدم InMemory للتوضيح.
/// </summary>
public interface IPropertyStore
{
    Task<Property> AddPropertyAsync(Property property, CancellationToken ct = default);
    Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Property>> SearchAsync(PropertySearchRequest request, CancellationToken ct = default);
    Task UpdatePropertyAsync(Property property, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<PropertyInquiry> AddInquiryAsync(PropertyInquiry inquiry, CancellationToken ct = default);
    Task<List<PropertyInquiry>> GetInquiriesAsync(Guid propertyId, CancellationToken ct = default);
}

/// <summary>
/// مخزن في الذاكرة مع بيانات تجريبية تشبه عشير
/// </summary>
public class InMemoryPropertyStore : IPropertyStore
{
    private readonly List<Property> _properties = new();
    private readonly List<PropertyInquiry> _inquiries = new();

    public InMemoryPropertyStore()
    {
        SeedData();
    }

    private void SeedData()
    {
        var ownerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var owner2Id = Guid.Parse("00000000-0000-0000-0000-000000000002");

        _properties.AddRange(new[]
        {
            new Property
            {
                Id = Guid.NewGuid(), Title = "شقة فاخرة في حي النرجس", Description = "شقة 3 غرف مع صالة ومطبخ، تشطيب سوبر لوكس",
                Category = "residential", PropertyType = "apartment", Purpose = "rent",
                City = "الرياض", District = "النرجس", Price = 35000, Area = 150, Rooms = 3, Bathrooms = 2, Floor = 4, Furnished = false,
                OwnerId = ownerId, OwnerName = "أحمد", OwnerPhone = "0501234567", Status = "active", CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Property
            {
                Id = Guid.NewGuid(), Title = "فيلا للبيع في حي الملقا", Description = "فيلا دوبلكس 5 غرف مع مسبح وحديقة",
                Category = "residential", PropertyType = "villa", Purpose = "sale",
                City = "الرياض", District = "الملقا", Price = 2500000, Area = 400, Rooms = 5, Bathrooms = 4, Furnished = true,
                OwnerId = ownerId, OwnerName = "أحمد", OwnerPhone = "0501234567", Status = "active", CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Property
            {
                Id = Guid.NewGuid(), Title = "مكتب تجاري في طريق الملك فهد", Description = "مكتب مؤثث بالكامل مع قاعة اجتماعات",
                Category = "commercial", PropertyType = "office", Purpose = "rent",
                City = "الرياض", District = "العليا", Price = 60000, Area = 80, Rooms = 3, Bathrooms = 1, Furnished = true,
                OwnerId = owner2Id, OwnerName = "سارة", OwnerPhone = "0559876543", Status = "active", CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Property
            {
                Id = Guid.NewGuid(), Title = "أرض سكنية في جدة", Description = "أرض مساحة 600 متر على شارعين",
                Category = "land", PropertyType = "land", Purpose = "sale",
                City = "جدة", District = "الحمراء", Price = 800000, Area = 600,
                OwnerId = owner2Id, OwnerName = "سارة", OwnerPhone = "0559876543", Status = "active", CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Property
            {
                Id = Guid.NewGuid(), Title = "شقة مفروشة للإيجار الشهري", Description = "شقة غرفتين مفروشة بالكامل قريبة من المترو",
                Category = "residential", PropertyType = "apartment", Purpose = "rent",
                City = "الرياض", District = "الورود", Price = 5500, Area = 90, Rooms = 2, Bathrooms = 1, Floor = 2, Furnished = true,
                OwnerId = ownerId, OwnerName = "أحمد", OwnerPhone = "0501234567", Status = "active", CreatedAt = DateTime.UtcNow
            },
            new Property
            {
                Id = Guid.NewGuid(), Title = "محل تجاري في الدمام", Description = "محل على شارع رئيسي مساحة 50 متر",
                Category = "commercial", PropertyType = "shop", Purpose = "rent",
                City = "الدمام", District = "الفيصلية", Price = 25000, Area = 50,
                OwnerId = owner2Id, OwnerName = "سارة", OwnerPhone = "0559876543", Status = "active", CreatedAt = DateTime.UtcNow.AddDays(-7)
            }
        });
    }

    public Task<Property> AddPropertyAsync(Property property, CancellationToken ct = default)
    {
        _properties.Add(property);
        return Task.FromResult(property);
    }

    public Task<Property?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_properties.FirstOrDefault(p => p.Id == id && !p.IsDeleted));

    public Task<List<Property>> SearchAsync(PropertySearchRequest request, CancellationToken ct = default)
    {
        var query = _properties.Where(p => !p.IsDeleted && p.Status == "active").AsEnumerable();

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

        query = request.OrderBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "area" => query.OrderByDescending(p => p.Area),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var results = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        return Task.FromResult(results);
    }

    public Task UpdatePropertyAsync(Property property, CancellationToken ct = default)
    {
        var idx = _properties.FindIndex(p => p.Id == property.Id);
        if (idx >= 0) _properties[idx] = property;
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var p = _properties.FirstOrDefault(p => p.Id == id);
        if (p != null) { p.IsDeleted = true; p.UpdatedAt = DateTime.UtcNow; }
        return Task.CompletedTask;
    }

    public Task<PropertyInquiry> AddInquiryAsync(PropertyInquiry inquiry, CancellationToken ct = default)
    {
        _inquiries.Add(inquiry);
        return Task.FromResult(inquiry);
    }

    public Task<List<PropertyInquiry>> GetInquiriesAsync(Guid propertyId, CancellationToken ct = default)
        => Task.FromResult(_inquiries.Where(i => i.PropertyId == propertyId && !i.IsDeleted).ToList());
}
