using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Orders.Entities;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Vendors.Entities;
using Order.Api.Models;
using System.Text.Json;

namespace Order.Api.Controllers;

/// <summary>
/// إدارة طلبات العميل مع دعم التوصيل للسيارة ومشاركة الموقع
/// </summary>
[ApiController]
[Route("api/customer/orders")]
[Authorize]
public class CustomerOrdersController : ControllerBase
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<CustomerOrdersController> _logger;

    public CustomerOrdersController(
        IRepositoryFactory repositoryFactory,
        ILogger<CustomerOrdersController> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    /// <summary>
    /// إنشاء طلب جديد
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        // التحقق من صحة البيانات
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { Message = "يجب إضافة عنصر واحد على الأقل" });

        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();
        var orderRepo = _repositoryFactory.CreateRepository<ACommerce.Orders.Entities.Order>();
        var orderItemRepo = _repositoryFactory.CreateRepository<OrderItem>();

        // التحقق من العروض والمتجر
        var firstListing = await listingRepo.GetByIdAsync(request.Items.First().OfferId);
        if (firstListing == null)
            return BadRequest(new { Message = "العرض غير موجود" });

        var vendor = await vendorRepo.GetByIdAsync(firstListing.VendorId);
        if (vendor == null)
            return BadRequest(new { Message = "المتجر غير موجود" });

        // حساب المجموع
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var listing = await listingRepo.GetByIdAsync(item.OfferId);
            if (listing == null)
                continue;

            if (listing.VendorId != firstListing.VendorId)
                return BadRequest(new { Message = "جميع العناصر يجب أن تكون من نفس المتجر" });

            var subtotal = listing.Price * item.Quantity;
            totalAmount += subtotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductListingId = listing.Id,
                Quantity = item.Quantity,
                Price = listing.Price,
                TotalPrice = subtotal,
                Notes = item.Notes
            });
        }

        // إنشاء الطلب
        var order = new ACommerce.Orders.Entities.Order
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            VendorId = vendor.Id,
            TotalAmount = totalAmount,
            Status = ACommerce.Orders.Enums.OrderStatus.Draft, // Pending
            Notes = request.CustomerNotes,
            // تخزين بيانات التوصيل في الـ Metadata
            MetadataJson = JsonSerializer.Serialize(new OrderMetadata
            {
                DeliveryType = request.DeliveryType,
                PaymentMethod = request.PaymentMethod,
                DeliveryLocation = request.DeliveryLocation,
                CarInfo = request.CarInfo
            })
        };

        await orderRepo.AddAsync(order);

        // إضافة العناصر
        foreach (var item in orderItems)
        {
            item.OrderId = order.Id;
            await orderItemRepo.AddAsync(item);
        }

        _logger.LogInformation("Order {OrderId} created by {ProfileId} for vendor {VendorId}",
            order.Id, profileId, vendor.Id);

        return Ok(new
        {
            OrderId = order.Id,
            OrderNumber = order.Id.ToString()[..8].ToUpper(),
            TotalAmount = totalAmount,
            Status = "pending",
            DeliveryType = request.DeliveryType.ToString(),
            PaymentMethod = request.PaymentMethod.ToString(),
            Vendor = new
            {
                vendor.Id,
                vendor.Name,
                vendor.ContactPhone
            },
            Message = "تم إنشاء الطلب بنجاح"
        });
    }

    /// <summary>
    /// تحديث موقع التوصيل (Live Location)
    /// </summary>
    [HttpPut("{orderId}/location")]
    public async Task<IActionResult> UpdateDeliveryLocation(Guid orderId, [FromBody] UpdateLocationRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var orderRepo = _repositoryFactory.CreateRepository<ACommerce.Orders.Entities.Order>();
        var order = await orderRepo.GetByIdAsync(orderId);

        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.ProfileId != profileId)
            return Forbid();

        // تحديث الموقع
        var metadata = string.IsNullOrEmpty(order.MetadataJson)
            ? new OrderMetadata()
            : JsonSerializer.Deserialize<OrderMetadata>(order.MetadataJson) ?? new OrderMetadata();

        metadata.DeliveryLocation = new DeliveryLocation
        {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LocationDescription = request.LocationDescription,
            IsLiveLocation = true,
            LastUpdated = DateTime.UtcNow
        };

        order.MetadataJson = JsonSerializer.Serialize(metadata);
        await orderRepo.UpdateAsync(order);

        _logger.LogInformation("Location updated for order {OrderId}", orderId);

        return Ok(new { Message = "تم تحديث الموقع" });
    }

    /// <summary>
    /// الحصول على طلبات العميل
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var orderRepo = _repositoryFactory.CreateRepository<ACommerce.Orders.Entities.Order>();
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();

        var orders = await orderRepo.FindAsync(o => o.ProfileId == profileId && !o.IsDeleted);
        var orderedList = orders.OrderByDescending(o => o.CreatedAt).ToList();

        var vendors = (await vendorRepo.GetAllAsync()).ToDictionary(v => v.Id);

        var results = new List<object>();
        foreach (var order in orderedList.Skip((page - 1) * pageSize).Take(pageSize))
        {
            var metadata = string.IsNullOrEmpty(order.MetadataJson)
                ? new OrderMetadata()
                : JsonSerializer.Deserialize<OrderMetadata>(order.MetadataJson) ?? new OrderMetadata();

            vendors.TryGetValue(order.VendorId, out var vendor);

            results.Add(new
            {
                order.Id,
                OrderNumber = order.Id.ToString()[..8].ToUpper(),
                order.TotalAmount,
                Status = MapOrderStatus(order.Status),
                StatusAr = MapOrderStatusArabic(order.Status),
                order.CreatedAt,
                metadata.DeliveryType,
                metadata.PaymentMethod,
                Vendor = vendor != null ? new
                {
                    vendor.Id,
                    vendor.Name,
                    vendor.ContactPhone
                } : null
            });
        }

        return Ok(new
        {
            Items = results,
            Total = orderedList.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// الحصول على تفاصيل طلب
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetails(Guid orderId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var orderRepo = _repositoryFactory.CreateRepository<ACommerce.Orders.Entities.Order>();
        var orderItemRepo = _repositoryFactory.CreateRepository<OrderItem>();
        var vendorRepo = _repositoryFactory.CreateRepository<Vendor>();
        var listingRepo = _repositoryFactory.CreateRepository<ProductListing>();

        var order = await orderRepo.GetByIdAsync(orderId);
        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.ProfileId != profileId)
            return Forbid();

        var vendor = await vendorRepo.GetByIdAsync(order.VendorId);
        var items = await orderItemRepo.FindAsync(i => i.OrderId == orderId);

        var metadata = string.IsNullOrEmpty(order.MetadataJson)
            ? new OrderMetadata()
            : JsonSerializer.Deserialize<OrderMetadata>(order.MetadataJson) ?? new OrderMetadata();

        var itemDetails = new List<object>();
        foreach (var item in items)
        {
            var listing = item.ProductListingId.HasValue
                ? await listingRepo.GetByIdAsync(item.ProductListingId.Value)
                : null;

            itemDetails.Add(new
            {
                item.Id,
                OfferTitle = listing?.Title,
                item.Quantity,
                item.Price,
                item.TotalPrice,
                item.Notes
            });
        }

        return Ok(new
        {
            order.Id,
            OrderNumber = order.Id.ToString()[..8].ToUpper(),
            order.TotalAmount,
            Status = MapOrderStatus(order.Status),
            StatusAr = MapOrderStatusArabic(order.Status),
            order.CreatedAt,
            order.Notes,
            DeliveryType = metadata.DeliveryType,
            PaymentMethod = metadata.PaymentMethod,
            DeliveryLocation = metadata.DeliveryLocation,
            CarInfo = metadata.CarInfo,
            Vendor = vendor != null ? new
            {
                vendor.Id,
                vendor.Name,
                vendor.NameEn,
                vendor.ContactPhone,
                vendor.Latitude,
                vendor.Longitude
            } : null,
            Items = itemDetails
        });
    }

    /// <summary>
    /// إلغاء طلب
    /// </summary>
    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var orderRepo = _repositoryFactory.CreateRepository<ACommerce.Orders.Entities.Order>();
        var order = await orderRepo.GetByIdAsync(orderId);

        if (order == null)
            return NotFound(new { Message = "الطلب غير موجود" });

        if (order.ProfileId != profileId)
            return Forbid();

        // لا يمكن إلغاء طلب قيد التحضير أو جاهز
        if (order.Status != ACommerce.Orders.Enums.OrderStatus.Draft)
            return BadRequest(new { Message = "لا يمكن إلغاء هذا الطلب" });

        order.Status = ACommerce.Orders.Enums.OrderStatus.Cancelled;
        await orderRepo.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} cancelled by customer", orderId);

        return Ok(new { Message = "تم إلغاء الطلب" });
    }

    private static string MapOrderStatus(ACommerce.Orders.Enums.OrderStatus status) => status switch
    {
        ACommerce.Orders.Enums.OrderStatus.Draft => "pending",
        ACommerce.Orders.Enums.OrderStatus.Confirmed => "accepted",
        ACommerce.Orders.Enums.OrderStatus.Processing => "preparing",
        ACommerce.Orders.Enums.OrderStatus.Shipped => "ready",
        ACommerce.Orders.Enums.OrderStatus.Delivered => "delivered",
        ACommerce.Orders.Enums.OrderStatus.Cancelled => "cancelled",
        _ => "unknown"
    };

    private static string MapOrderStatusArabic(ACommerce.Orders.Enums.OrderStatus status) => status switch
    {
        ACommerce.Orders.Enums.OrderStatus.Draft => "في الانتظار",
        ACommerce.Orders.Enums.OrderStatus.Confirmed => "تم القبول",
        ACommerce.Orders.Enums.OrderStatus.Processing => "قيد التحضير",
        ACommerce.Orders.Enums.OrderStatus.Shipped => "جاهز للاستلام",
        ACommerce.Orders.Enums.OrderStatus.Delivered => "تم التسليم",
        ACommerce.Orders.Enums.OrderStatus.Cancelled => "ملغي",
        _ => "غير معروف"
    };
}

public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public DeliveryType DeliveryType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CustomerNotes { get; set; }
    public DeliveryLocation? DeliveryLocation { get; set; }
    public CarInfo? CarInfo { get; set; }
}

public class OrderItemRequest
{
    public Guid OfferId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationDescription { get; set; }
}

public class OrderMetadata
{
    public DeliveryType DeliveryType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DeliveryLocation? DeliveryLocation { get; set; }
    public CarInfo? CarInfo { get; set; }
}
