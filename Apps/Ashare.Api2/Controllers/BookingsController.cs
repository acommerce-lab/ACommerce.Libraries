using ACommerce.OperationEngine.Analyzers;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Patterns;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api2.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api2.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBaseAsyncRepository<Booking> _bookings;
    private readonly IBaseAsyncRepository<Listing> _listings;
    private readonly IBaseAsyncRepository<User> _users;
    private readonly OpEngine _engine;

    public BookingsController(IRepositoryFactory factory, OpEngine engine)
    {
        _bookings = factory.CreateRepository<Booking>();
        _listings = factory.CreateRepository<Listing>();
        _users    = factory.CreateRepository<User>();
        _engine   = engine;
    }

    public record CreateBookingRequest(
        Guid ListingId,
        Guid CustomerId,
        DateTime StartDate,
        DateTime EndDate,
        string? Notes);

    /// <summary>
    /// إنشاء حجز - يُمثل كقيد محاسبي:
    /// العميل (مدين) ← العرض/المالك (دائن) بقيمة الحجز.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req, CancellationToken ct)
    {
        var listing = await _listings.GetByIdAsync(req.ListingId, ct);
        if (listing == null) return NotFound(new { error = "listing_not_found" });

        if (listing.Status != ListingStatus.Published)
            return BadRequest(new { error = "listing_not_available" });

        var customer = await _users.GetByIdAsync(req.CustomerId, ct);
        if (customer == null) return BadRequest(new { error = "customer_not_found" });

        var totalPrice = listing.Price * Math.Max(1, (req.EndDate - req.StartDate).Days / 30);

        // === باني العملية المحاسبية ===
        // قيد: العميل (مدين) ← المالك (دائن) بقيمة الحجز
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ListingId = listing.Id,
            CustomerId = customer.Id,
            OwnerId = listing.OwnerId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalPrice = totalPrice,
            Currency = listing.Currency,
            Notes = req.Notes,
            Status = BookingStatus.Pending
        };

        var op = Entry.Create("booking.create")
            .Describe($"Booking #{booking.Id} by Customer:{customer.Id} for Listing:{listing.Id}")
            .From($"Customer:{customer.Id}", totalPrice,
                ("role", "customer"),
                ("booking_status", "pending"))
            .To($"Listing:{listing.Id}", totalPrice,
                ("role", "listing"),
                ("owner_id", listing.OwnerId.ToString()))
            .Tag("booking_id", booking.Id.ToString())
            .Tag("listing_id", listing.Id.ToString())
            .Tag("category_id", listing.CategoryId.ToString())
            .Tag("currency", listing.Currency)
            // محلل: تواريخ الحجز يجب أن تكون منطقية
            .Analyze(new ConditionAnalyzer(
                "valid_date_range",
                _ => req.EndDate > req.StartDate,
                "end_date_must_be_after_start_date"))
            .Execute(async ctx =>
            {
                // تعديل بيانات حسب نتيجة العملية وحفظها
                await _bookings.AddAsync(booking, ctx.CancellationToken);

                // قفل العرض من حجوزات أخرى
                listing.Status = ListingStatus.Reserved;
                await _listings.UpdateAsync(listing, ctx.CancellationToken);

                ctx.Set("bookingId", booking.Id);
            })
            .OnAfterComplete(async ctx =>
            {
                booking.Status = BookingStatus.AwaitingPayment;
                booking.OperationId = ctx.Operation.Id;
                await _bookings.UpdateAsync(booking, ctx.CancellationToken);
            })
            .Build();

        var result = await _engine.ExecuteAsync(op, ct);

        if (!result.Success)
        {
            // إرجاع حالة العرض إذا فشلت العملية
            if (listing.Status == ListingStatus.Reserved)
            {
                listing.Status = ListingStatus.Published;
                await _listings.UpdateAsync(listing, ct);
            }
            return BadRequest(new
            {
                error = "booking_failed",
                operationStatus = (result.Success ? "Success" : (result.IsPartial ? "Partial" : "Failed")),
                validationErrors = result.Context?.Items.GetValueOrDefault("_errors")
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, new
        {
            booking,
            operationId = op.Id,
            opStatus = (result.Success ? "Success" : (result.IsPartial ? "Partial" : "Failed"))
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var b = await _bookings.GetByIdAsync(id, ct);
        return b == null ? NotFound() : Ok(b);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    public async Task<IActionResult> ByCustomer(Guid customerId, CancellationToken ct)
    {
        var list = await _bookings.GetAllWithPredicateAsync(b => b.CustomerId == customerId);
        return Ok(list);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var booking = await _bookings.GetByIdAsync(id, ct);
        if (booking == null) return NotFound();

        if (booking.Status == BookingStatus.Paid || booking.Status == BookingStatus.Completed)
            return BadRequest(new { error = "cannot_cancel_completed" });

        booking.Status = BookingStatus.Cancelled;
        await _bookings.UpdateAsync(booking, ct);

        // إعادة إتاحة العرض
        var listing = await _listings.GetByIdAsync(booking.ListingId, ct);
        if (listing != null && listing.Status == ListingStatus.Reserved)
        {
            listing.Status = ListingStatus.Published;
            await _listings.UpdateAsync(listing, ct);
        }

        return Ok(booking);
    }
}
