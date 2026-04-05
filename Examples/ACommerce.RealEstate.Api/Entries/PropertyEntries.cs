using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Builder;
using ACommerce.RealEstate.Api.Entities;

namespace ACommerce.RealEstate.Api.Entries;

/// <summary>
/// جميع عمليات العقارات كقيود محاسبية.
/// كل قيد يحتوي طبقة DB تمر عبر SharedKernel Repository.
/// </summary>
public static class PropertyEntries
{
    public static Entry ListProperty(Property property, IPropertyStore store)
    {
        return EntryBuilder.Create("property.list")
            .Describe($"List: {property.Title}")
            .From($"Owner:{property.OwnerId}", "Property", 1)
            .To("Platform", "Property", 1)
            .Validate(ctx =>
            {
                if (string.IsNullOrWhiteSpace(property.Title)) { ctx.Set("_validationError", "Title required"); return false; }
                if (property.Price <= 0) { ctx.Set("_validationError", "Price must be positive"); return false; }
                if (string.IsNullOrWhiteSpace(property.City)) { ctx.Set("_validationError", "City required"); return false; }
                return true;
            })
            // طبقة DB: حفظ عبر SharedKernel
            .Execute(async ctx =>
            {
                property.Id = Guid.NewGuid();
                property.CreatedAt = DateTime.UtcNow;
                property.Status = "active";
                var saved = await store.AddPropertyAsync(property, ctx.CancellationToken);
                ctx.Set("property", saved);
            })
            .Build();
    }

    public static Entry SearchProperties(PropertySearchRequest request, IPropertyStore store)
    {
        return EntryBuilder.Create("property.search")
            .Describe($"Search: {request.City ?? "all"}")
            .From($"User:{request.UserId ?? "anon"}", "SearchRequest", 1)
            .To("Platform", "SearchResult", 1)
            // طبقة DB: استعلام عبر SharedKernel Repository
            .Execute(async ctx =>
            {
                var (items, total) = await store.SearchAsync(request, ctx.CancellationToken);
                ctx.Set("results", items);
                ctx.Set("totalCount", total);
            })
            .Build();
    }

    public static Entry ViewProperty(Guid propertyId, IPropertyStore store)
    {
        return EntryBuilder.Create("property.view")
            .From("User", "ViewRequest", 1)
            .To("Platform", "PropertyDetails", 1)
            .Execute(async ctx =>
            {
                var property = await store.GetByIdAsync(propertyId, ctx.CancellationToken);
                if (property == null) throw new KeyNotFoundException($"Property {propertyId} not found");
                ctx.Set("property", property);
            })
            .Build();
    }

    public static Entry UpdateProperty(Guid propertyId, Action<Property> updates, IPropertyStore store)
    {
        return EntryBuilder.Create("property.update")
            .From("Owner", "UpdateRequest", 1)
            .To("Platform", "UpdateResult", 1)
            .Validate(async ctx =>
            {
                var existing = await store.GetByIdAsync(propertyId, ctx.CancellationToken);
                if (existing == null) { ctx.Set("_validationError", "Not found"); return false; }
                ctx.Set("existing", existing);
                return true;
            })
            .Execute(async ctx =>
            {
                var existing = ctx.Get<Property>("existing");
                updates(existing);
                existing.UpdatedAt = DateTime.UtcNow;
                await store.UpdatePropertyAsync(existing, ctx.CancellationToken);
                ctx.Set("property", existing);
            })
            .Build();
    }

    public static Entry SendInquiry(PropertyInquiry inquiry, IPropertyStore store)
    {
        return EntryBuilder.Create("property.inquiry")
            .From($"User:{inquiry.UserId}", "Inquiry", 1)
            .To($"Owner:property_{inquiry.PropertyId}", "Inquiry", 1)
            .Validate(async ctx =>
            {
                var property = await store.GetByIdAsync(inquiry.PropertyId, ctx.CancellationToken);
                if (property == null) { ctx.Set("_validationError", "Not found"); return false; }
                if (property.Status != "active") { ctx.Set("_validationError", "Not active"); return false; }
                return true;
            })
            .Execute(async ctx =>
            {
                inquiry.Id = Guid.NewGuid();
                inquiry.CreatedAt = DateTime.UtcNow;
                await store.AddInquiryAsync(inquiry, ctx.CancellationToken);
                ctx.Set("inquiry", inquiry);
            })
            .WithSubEntry("notify.owner", sub =>
            {
                sub.From("Platform", "Notification", 1)
                   .To($"Owner:property_{inquiry.PropertyId}", "Notification", 1)
                   .Execute(ctx => { ctx.Set("notified", true); });
            })
            .Build();
    }

    public static Entry RemoveProperty(Guid propertyId, IPropertyStore store)
    {
        return EntryBuilder.Create("property.remove")
            .From("Owner", "RemoveRequest", 1)
            .To("Platform", "RemoveResult", 1)
            .Execute(async ctx =>
            {
                await store.SoftDeleteAsync(propertyId, ctx.CancellationToken);
                ctx.Set("removed", true);
            })
            .Build();
    }
}

public class PropertySearchRequest
{
    public string? UserId { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Purpose { get; set; }
    public string? Category { get; set; }
    public string? PropertyType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public int? MinRooms { get; set; }
    public bool? Furnished { get; set; }
    public string? OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
