using ACommerce.AccountingKernel.Abstractions;
using ACommerce.AccountingKernel.Builder;
using ACommerce.RealEstate.Api.Entities;

namespace ACommerce.RealEstate.Api.Entries;

/// <summary>
/// جميع عمليات العقارات مُعرّفة كقيود محاسبية.
/// كل عملية = أطراف + تحقق + تنفيذ + أحداث.
/// المحرك يتولى دورة الحياة والتوثيق.
/// </summary>
public static class PropertyEntries
{
    /// <summary>
    /// قيد: نشر عقار جديد
    /// المالك (مدين) يقدم عقاراً → المنصة (دائن) تستلمه
    /// </summary>
    public static Entry ListProperty(Property property, IPropertyStore store)
    {
        return EntryBuilder.Create("property.list")
            .Describe($"List property: {property.Title} in {property.City}")
            .From($"Owner:{property.OwnerId}", "Property", 1)
            .To("Platform", "Property", 1)

            .Validate(ctx =>
            {
                if (string.IsNullOrWhiteSpace(property.Title)) { ctx.Set("_validationError", "Title is required"); return false; }
                if (property.Price <= 0) { ctx.Set("_validationError", "Price must be positive"); return false; }
                if (string.IsNullOrWhiteSpace(property.City)) { ctx.Set("_validationError", "City is required"); return false; }
                return true;
            })

            .Execute(async ctx =>
            {
                property.Id = Guid.NewGuid();
                property.CreatedAt = DateTime.UtcNow;
                property.Status = "active";
                await store.AddPropertyAsync(property, ctx.CancellationToken);
                ctx.Set("property", property);
            })

            .Build();
    }

    /// <summary>
    /// قيد: البحث عن عقارات
    /// المستخدم (مدين) يطلب بحثاً → المنصة (دائن) تقدم نتائج
    /// </summary>
    public static Entry SearchProperties(PropertySearchRequest request, IPropertyStore store)
    {
        return EntryBuilder.Create("property.search")
            .Describe($"Search: {request.City ?? "all"} / {request.Purpose ?? "all"}")
            .From($"User:{request.UserId ?? "anonymous"}", "SearchRequest", 1)
            .To("Platform", "SearchResult", 1)

            .Execute(async ctx =>
            {
                var results = await store.SearchAsync(request, ctx.CancellationToken);
                ctx.Set("results", results);
                ctx.Set("count", results.Count);
            })

            .Build();
    }

    /// <summary>
    /// قيد: عرض تفاصيل عقار
    /// المستخدم (مدين) يطلب تفاصيل → المنصة (دائن) تقدم معلومات
    /// </summary>
    public static Entry ViewProperty(Guid propertyId, string? userId, IPropertyStore store)
    {
        return EntryBuilder.Create("property.view")
            .Describe($"View property {propertyId}")
            .From($"User:{userId ?? "anonymous"}", "ViewRequest", 1)
            .To("Platform", "PropertyDetails", 1)

            .Execute(async ctx =>
            {
                var property = await store.GetByIdAsync(propertyId, ctx.CancellationToken);
                if (property == null)
                    throw new KeyNotFoundException($"Property {propertyId} not found");
                ctx.Set("property", property);
            })

            .Build();
    }

    /// <summary>
    /// قيد: تحديث عقار
    /// المالك (مدين) يعدّل بيانات → المنصة (دائن) تحفظ التعديل
    /// </summary>
    public static Entry UpdateProperty(Guid propertyId, Action<Property> updates, IPropertyStore store)
    {
        return EntryBuilder.Create("property.update")
            .Describe($"Update property {propertyId}")
            .From("Owner", "UpdateRequest", 1)
            .To("Platform", "UpdateResult", 1)

            .Validate(async ctx =>
            {
                var existing = await store.GetByIdAsync(propertyId, ctx.CancellationToken);
                if (existing == null) { ctx.Set("_validationError", "Property not found"); return false; }
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

    /// <summary>
    /// قيد: إرسال استفسار عن عقار
    /// المستخدم (مدين) يُرسل استفساراً → المالك (دائن) يستلمه
    /// مع قيد فرعي: إشعار للمالك
    /// </summary>
    public static Entry SendInquiry(PropertyInquiry inquiry, IPropertyStore store)
    {
        return EntryBuilder.Create("property.inquiry")
            .Describe($"Inquiry on property {inquiry.PropertyId}")
            .From($"User:{inquiry.UserId}", "Inquiry", 1)
            .To($"Owner:property_{inquiry.PropertyId}", "Inquiry", 1)

            .Validate(async ctx =>
            {
                var property = await store.GetByIdAsync(inquiry.PropertyId, ctx.CancellationToken);
                if (property == null) { ctx.Set("_validationError", "Property not found"); return false; }
                if (property.Status != "active") { ctx.Set("_validationError", "Property is not active"); return false; }
                ctx.Set("property", property);
                return true;
            })

            .Execute(async ctx =>
            {
                inquiry.Id = Guid.NewGuid();
                inquiry.CreatedAt = DateTime.UtcNow;
                await store.AddInquiryAsync(inquiry, ctx.CancellationToken);
                ctx.Set("inquiry", inquiry);
            })

            // قيد فرعي: إشعار المالك
            .WithSubEntry("notify.owner", sub =>
            {
                sub.From("Platform", "Notification", 1)
                   .To($"Owner:property_{inquiry.PropertyId}", "Notification", 1)
                   .Execute(ctx =>
                   {
                       // في تطبيق حقيقي: إرسال push notification
                       ctx.Set("notified", true);
                   });
            })

            .Build();
    }

    /// <summary>
    /// قيد: حذف عقار (منطقي)
    /// المالك (مدين) يسحب عقاره → المنصة (دائن) تخفيه
    /// </summary>
    public static Entry RemoveProperty(Guid propertyId, IPropertyStore store)
    {
        return EntryBuilder.Create("property.remove")
            .Describe($"Remove property {propertyId}")
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

/// <summary>
/// طلب البحث عن عقارات
/// </summary>
public class PropertySearchRequest
{
    public string? UserId { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Purpose { get; set; }          // sale, rent
    public string? Category { get; set; }         // residential, commercial, land
    public string? PropertyType { get; set; }     // apartment, villa, office
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public int? MinRooms { get; set; }
    public bool? Furnished { get; set; }
    public string? OrderBy { get; set; }          // price_asc, price_desc, newest, area
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
