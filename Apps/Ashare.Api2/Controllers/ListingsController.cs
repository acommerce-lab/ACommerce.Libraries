using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Ashare.Api2.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api2.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly IBaseAsyncRepository<Listing> _repo;
    private readonly SubscriptionGuard _guard;
    private readonly OpEngine _engine;

    public ListingsController(IRepositoryFactory factory, SubscriptionGuard guard, OpEngine engine)
    {
        _repo = factory.CreateRepository<Listing>();
        _guard = guard;
        _engine = engine;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? city,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _repo.GetPagedAsync(
            pageNumber: page,
            pageSize: pageSize,
            predicate: l =>
                l.Status == ListingStatus.Published &&
                (categoryId == null || l.CategoryId == categoryId) &&
                (city == null || l.City == city) &&
                (minPrice == null || l.Price >= minPrice) &&
                (maxPrice == null || l.Price <= maxPrice),
            orderBy: l => l.PublishedAt!,
            ascending: false);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var listing = await _repo.GetByIdAsync(id, ct);
        if (listing == null) return NotFound();

        // عداد المشاهدات
        listing.ViewCount++;
        await _repo.UpdateAsync(listing, ct);

        return Ok(listing);
    }

    public record CreateListingRequest(
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListingRequest req, CancellationToken ct)
    {
        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            OwnerId = req.OwnerId,
            CategoryId = req.CategoryId,
            Title = req.Title,
            Description = req.Description,
            Price = req.Price,
            Duration = req.Duration,
            TimeUnit = req.TimeUnit,
            City = req.City,
            District = req.District,
            PropertyType = req.PropertyType,
            Floor = req.Floor,
            Area = req.Area,
            Rooms = req.Rooms,
            Bathrooms = req.Bathrooms,
            Furnished = req.Furnished,
            LicenseNumber = req.LicenseNumber,
            Status = ListingStatus.Draft
        };

        // === القيد المحاسبي مع SubscriptionAnalyzer كـ PreAnalyzer ===
        // المالك (مدين) ← الفئة (دائن) - فحص الاشتراك يتم داخل المحرك قبل التنفيذ
        var op = Entry.Create("listing.create")
            .Describe($"Owner:{req.OwnerId} creates listing in Category:{req.CategoryId}")
            .From($"User:{req.OwnerId}", 1, ("role", "owner"))
            .To($"Category:{req.CategoryId}", 1, ("role", "category"))
            .Tag("listing_id", listing.Id.ToString())
            .Tag("category_id", req.CategoryId.ToString())
            .Tag("subscription_check", "create_listing")
            .Analyze(new SubscriptionAnalyzer(
                _guard, req.OwnerId, SubscriptionCheckKind.CreateListing, req.CategoryId))
            .Execute(async ctx =>
            {
                // المحلل تم تشغيله بنجاح قبل وصولنا هنا - الباقة تسمح
                listing.Status = ListingStatus.Published;
                listing.PublishedAt = DateTime.UtcNow;
                await _repo.AddAsync(listing, ctx.CancellationToken);
            })
            .Build();

        var result = await _engine.ExecuteAsync(op, ct);

        if (!result.Success)
        {
            // فشل المحلل أو التنفيذ
            return StatusCode(403, new
            {
                error = "listing_create_blocked",
                analyzerErrors = result.ValidationErrors,
                failedAnalyzer = result.FailedAnalyzer,
                message = result.ErrorMessage,
                hint = "اشترك في باقة تدعم هذه الفئة من /api/plans"
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = listing.Id }, new
        {
            listing,
            operationId = op.Id
        });
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var listing = await _repo.GetByIdAsync(id, ct);
        if (listing == null) return NotFound();

        listing.Status = ListingStatus.Published;
        listing.PublishedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(listing, ct);
        return Ok(listing);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _repo.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}
