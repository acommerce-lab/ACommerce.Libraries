using ACommerce.Client.Http;
using ACommerce.Client.Operations;
using ACommerce.OperationEngine.Patterns;
using ACommerce.OperationEngine.Wire;

namespace ACommerce.Client.Domain.Listings;

// ═══════════════════════════════════════════════════════════════
// Intents
// ═══════════════════════════════════════════════════════════════

public record CreateListingIntent(
    Guid OwnerId,
    Guid CategoryId,
    string Title,
    string Description,
    decimal Price,
    int Duration,
    string TimeUnit,
    string City,
    string? District,
    string? PropertyType,
    int? Floor,
    double? Area,
    int? Rooms,
    int? Bathrooms,
    bool? Furnished,
    string? LicenseNumber);

public record ListingDto(
    Guid Id,
    Guid OwnerId,
    Guid CategoryId,
    string Title,
    string Description,
    decimal Price,
    string City,
    int Status,
    DateTime? PublishedAt,
    Guid? SubscriptionId,
    Guid? PlanIdSnapshot);

/// <summary>
/// عميل العروض - كل طريقة تبني قيد Entry محاسبياً وتدفعه عبر HttpDispatcher.
/// المحللات (SubscriptionLink / QuotaConsumption) كلها يُشغّلها الخادم،
/// والعميل يتلقى القيد الكامل في OperationEnvelope.
/// </summary>
public class ListingsClient
{
    private readonly ClientOpEngine _engine;

    public ListingsClient(ClientOpEngine engine) => _engine = engine;

    public async Task<OperationEnvelope<ListingDto>> CreateAsync(
        CreateListingIntent intent, CancellationToken ct = default)
    {
        // نبني نفس قيد الخادم الذي يُسجله ListingsController:
        //   From: User:{owner}  →  To: Category:{categoryId}
        // ولكن بدون محللات (هي تعمل على الخادم).
        var op = Entry.Create("listing.create")
            .Describe($"Owner:{intent.OwnerId} creates listing in Category:{intent.CategoryId}")
            .From($"User:{intent.OwnerId}", 1, ("role", "owner"))
            .To($"Category:{intent.CategoryId}", 1, ("role", "category"))
            .Tag("category_id", intent.CategoryId.ToString())
            .Tag("subscription_check", "create_listing")
            .Build();

        return await _engine.ExecuteAsync<ListingDto>(op, intent, ct);
    }

    public async Task<OperationEnvelope<object>> ListAsync(
        Guid? categoryId = null,
        string? city = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var op = Entry.Create("listing.list")
            .Describe("List listings")
            .From("Client:anonymous", 1, ("role", "viewer"))
            .To("Server:ashare", 1, ("role", "source"))
            .Tag("page", page.ToString())
            .Tag("page_size", pageSize.ToString())
            .Build();

        return await _engine.ExecuteAsync<object>(op, null, ct);
    }

    public async Task<OperationEnvelope<ListingDto>> GetAsync(Guid id, CancellationToken ct = default)
    {
        var op = Entry.Create("listing.get")
            .Describe($"Get listing {id}")
            .From("Client:anonymous", 1, ("role", "viewer"))
            .To($"Listing:{id}", 1, ("role", "target"))
            .Tag("listing_id", id.ToString())
            .Build();

        return await _engine.ExecuteAsync<ListingDto>(op, null, ct);
    }

    public static void RegisterRoutes(HttpRouteRegistry routes)
    {
        routes.Map("listing.create",  HttpMethod.Post, "/api/listings");
        routes.Map("listing.list",    HttpMethod.Get,  "/api/listings");
        routes.Map("listing.get",     HttpMethod.Get,  "/api/listings"); // سيُلحق الـ id يدوياً
        routes.Map("listing.publish", HttpMethod.Post, "/api/listings");
        routes.Map("listing.delete",  HttpMethod.Delete, "/api/listings");
    }
}
